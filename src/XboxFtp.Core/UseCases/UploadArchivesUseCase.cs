using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using Ionic.Crc;
using Ionic.Zip;
using Serilog;
using XboxFtp.Core.Entities;
using XboxFtp.Core.Ports.Notification;
using XboxFtp.Core.Ports.Persistence;

namespace XboxFtp.Core.UseCases
{
    public class UploadArchivesUseCase
    {
        private readonly IXboxGameRepositoryFactory _xboxGameRepositoryFactory;
        private readonly IProgressNotifier _notifier;
        private readonly BlockingCollection<IXboxTransferRequest> _xboxFtpRequests;
        private readonly BlockingCollection<XboxDirectoryCreateRequest> _xboxDirectoryCreateRequests;

        public UploadArchivesUseCase(IXboxGameRepositoryFactory xboxGameRepositoryFactory,
            IProgressNotifier notifier)
        {
            _xboxGameRepositoryFactory = xboxGameRepositoryFactory;
            _notifier = notifier;
            _xboxFtpRequests = new BlockingCollection<IXboxTransferRequest>();
            _xboxDirectoryCreateRequests = new BlockingCollection<XboxDirectoryCreateRequest>();
        }

        public void Execute(List<string> archivePaths)
        {
            try
            {
                NotifyAllGames(archivePaths);

                foreach (var archivePath in archivePaths)
                {
                    string gameName = GetGameNameFromPath(archivePath);
                    _notifier.StartingGameUpload(gameName);

                    try
                    {
                        Stopwatch uploadDuration = Stopwatch.StartNew();
                        var filesToUpload = GetFilesToUpload(gameName, archivePath);

                        CreateFolderStructure(gameName, archivePath);
                        UploadAllFiles(gameName, filesToUpload);

                        _notifier.FinishedGameUpload(gameName, uploadDuration.Elapsed);
                    }
                    catch (Exception ex)
                    {
                        _notifier.GameUploadError(gameName, ex, $"An unhandled exception occurred while uploading archive for {gameName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unhandled exception occurred while uploading archives");
            }
        }

        private void NotifyAllGames(List<string> archivePaths)
        {
            foreach (var archivePath in archivePaths)
            {
                string gameName = GetGameNameFromPath(archivePath);
                _notifier.GameAddedToUploadQueue(gameName);
            }
        }

        private static string GetGameNameFromPath(string archivePath)
        {
            return Path.GetFileNameWithoutExtension(archivePath);
        }

        private IList<IZipEntry> GetFilesToUpload(string gameName, string archivePath)
        {
            IXboxGameRepository xboxGameRepository = _xboxGameRepositoryFactory.Create();
            xboxGameRepository.Connect();

            IList<IZipEntry> filesToUpload; 
            
            using (ZipFile zip = ZipFile.Read(archivePath))
            {
                // Order files alphabetically to ensure we can resume interrupted uploads
                var files = new List<IZipEntry>(zip.Where(entry => !entry.IsDirectory).Select(x => new ZipEntryWrapper(x)).OrderBy(entry => entry.FileName));
                _notifier.CheckingForUploadedFiles(gameName);
                
                IUploadResumeStrategy uploadResumeStrategy = new BinarySearchUploadResumeStrategy(files, _notifier, gameName, xboxGameRepository);
                
                filesToUpload = uploadResumeStrategy.GetRemainingFiles();
            }

            xboxGameRepository.Disconnect();

            return filesToUpload;
        }

        private void UploadAllFiles(string gameName, IList<IZipEntry> filesToUpload)
        {
            long totalSizeToUpload = filesToUpload.Sum(x => x.UncompressedSize);
            ReportTotalBytesToUpload(gameName, totalSizeToUpload);

            BlockingCollection<IXboxTransferRequest> finishedRequests = new BlockingCollection<IXboxTransferRequest>();

            XboxTransferWorker fileWorker1 = new XboxTransferWorker(_xboxGameRepositoryFactory, gameName, _xboxFtpRequests, finishedRequests,_notifier);
            XboxTransferWorker fileWorker2 = new XboxTransferWorker(_xboxGameRepositoryFactory, gameName, _xboxFtpRequests, finishedRequests, _notifier);
            XboxTransferProgressWorker progressWorker = new XboxTransferProgressWorker(_xboxGameRepositoryFactory, gameName,_notifier, finishedRequests,
                totalSizeToUpload);

            progressWorker.Start();
            fileWorker1.Start();
            fileWorker2.Start();

            _notifier.ReportTotalFilesToTransfer(gameName, filesToUpload.Count);

            foreach (var zipEntry in filesToUpload)
            {
                WaitIfMaxOutstandingRequests();
                WaitIfMaxMemoryInQueue();

                if (zipEntry.UncompressedSize > 14572800)
                {
                    ExtractZipToDisk(zipEntry, gameName);
                }
                else
                {
                    ExtractZipToMemory(zipEntry, gameName);
                }
            }

            _notifier.WaitingForUploadsToComplete(gameName);
            WaitForRequestsToComplete();
            fileWorker1.Stop();
            fileWorker2.Stop();
        }

        private void ReportTotalBytesToUpload(string gameName, long totalBytesToUpload)
        {
            _notifier.ReportTotalBytesToUpload(gameName, totalBytesToUpload);
        }

        private void ExtractZipToDisk(IZipEntry zipEntry, string gameName)
        {
            _notifier.ExtractFileToDisk(gameName, zipEntry.FileName);

            var xboxTempFileDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "XboxFtp", "Temp", gameName);

            zipEntry.Extract(xboxTempFileDirectory, ExtractExistingFileAction.OverwriteSilently);

            XboxTempFileTransferRequest request = new XboxTempFileTransferRequest()
            {
                Path = zipEntry.FileName,
                TempFilePath = Path.Combine(xboxTempFileDirectory, zipEntry.FileName)
            };

            _notifier.AddingToUploadQueue(gameName, zipEntry.FileName);
            _xboxFtpRequests.Add(request);
        }

        private void ExtractZipToMemory(IZipEntry zipEntry, string gameName)
        {
            using (Stream reader = zipEntry.OpenReader())
            {
                byte[] data;
                data = new byte[zipEntry.UncompressedSize];
                reader.Read(data, 0, (int) zipEntry.UncompressedSize);
                
                XboxInMemoryTransferRequest request = new XboxInMemoryTransferRequest()
                {
                    Path = zipEntry.FileName,
                    Data = data
                };
                reader.Close();
                _notifier.AddingToUploadQueue(gameName, zipEntry.FileName);
                _xboxFtpRequests.Add(request);
            }
        }

        /// <summary>
        /// This should work but there seems to be an issue with the stream being used in the ftp client implementation
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="zipEntry"></param>
        private void StreamFromZip(ILogger logger, ZipEntry zipEntry)
        {
            XboxZipStreamTransferRequest request = new XboxZipStreamTransferRequest()
            {
                Path = zipEntry.FileName,
                ZipEntry = zipEntry
            };
            logger.Debug("Requesting upload for {FileName}", zipEntry.FileName);
            _xboxFtpRequests.Add(request);
        }

        private void WaitIfMaxOutstandingRequests()
        {
            while (_xboxFtpRequests.Count >= 10)
            {
                Thread.Sleep(500);
            }
        }

        private void WaitIfMaxMemoryInQueue()
        {
            while (GetQueueSize() > 314572800)
            {
                Thread.Sleep(1000);
            }
        }

        private long GetQueueSize()
        {
            var queueSize = _xboxFtpRequests.Sum(x => x.Length);
            return queueSize;
        }

        private void WaitForRequestsToComplete()
        {
            while (_xboxFtpRequests.Count > 0)
            {
                Log.Information("Waiting for upload to finish. {UploadRequestsRemaining}", _xboxFtpRequests.Count);
                Thread.Sleep(1000);
            }
        }

        private void WaitForDirectoryRequestsToComplete()
        {
            while (_xboxDirectoryCreateRequests.Count > 0)
            {
                Log.Information("Waiting for directories to be created. {UploadRequestsRemaining}", _xboxDirectoryCreateRequests.Count);
                Thread.Sleep(1000);
            }
        }

        private void CreateFolderStructure(string gameName, string archivePath)
        {
            _notifier.CreateFolderStructure(gameName);
            
            using (ZipFile zip = ZipFile.Read(archivePath))
            {
                var directories = zip.Where(entry => entry.IsDirectory);

                foreach (var zipEntry in directories)
                {
                    XboxDirectoryCreateRequest request = new XboxDirectoryCreateRequest()
                    {
                        Path = zipEntry.FileName
                    };

                    _xboxDirectoryCreateRequests.Add(request);
                }
            }

            XboxDirectoryStructureWorker folderWorker = new XboxDirectoryStructureWorker(_xboxGameRepositoryFactory, gameName, _xboxDirectoryCreateRequests);
            folderWorker.Start();
            WaitForDirectoryRequestsToComplete();
            _notifier.FinishedCreatingFolderStructure(gameName);
            folderWorker.Stop();
        }
    }
}
