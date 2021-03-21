using System;
using System.Collections;
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
using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;
using XboxFtp.Core.Entities;
using XboxFtp.Core.Ports.Notification;
using XboxFtp.Core.Ports.Persistence;

namespace XboxFtp.Core.UseCases
{
    public class UploadArchivesUseCase
    {
        private readonly IXboxGameRepositoryFactory _xboxGameRepositoryFactory;
        private readonly IProgressNotifier _notifier;
        private readonly IZipFileProcessor _zipFileProcessor;
        private readonly BlockingCollection<IXboxTransferRequest> _xboxFtpRequests;
        private readonly BlockingCollection<XboxDirectoryCreateRequest> _xboxDirectoryCreateRequests;

        public UploadArchivesUseCase(IXboxGameRepositoryFactory xboxGameRepositoryFactory,
            IProgressNotifier notifier, IZipFileProcessor zipFileProcessor)
        {
            _xboxGameRepositoryFactory = xboxGameRepositoryFactory;
            _notifier = notifier;
            _zipFileProcessor = zipFileProcessor;
            _xboxFtpRequests = new BlockingCollection<IXboxTransferRequest>();
            _xboxDirectoryCreateRequests = new BlockingCollection<XboxDirectoryCreateRequest>();
        }

        public void Execute(List<string> archivePaths)
        {
            try
            {
                NotifyAllGames(archivePaths);

                foreach (string archivePath in archivePaths)
                {
                    string gameName = GetGameNameFromPath(archivePath);
                    
                    ILogEventEnricher[] properties = new ILogEventEnricher[]
                    {
                        new PropertyEnricher("ArchivePath", archivePath),
                        new PropertyEnricher("Game", gameName)
                    };
                    
                    using (LogContext.Push(properties))
                    {
                        _notifier.StartingGameUpload(gameName);

                        try
                        {
                            Stopwatch uploadDuration = Stopwatch.StartNew();
                            var filesInArchive = GetAllFilesInArchive(gameName, archivePath);
                            long totalSizeOfArchive = filesInArchive.Sum(x => x.UncompressedSize);
                            
                            var uploadResumeReport = GetFilesToUpload(gameName, archivePath);
                            long totalSizeFilesToUpload = uploadResumeReport.RemainingFiles.Sum(x => x.UncompressedSize);

                            Log.ForContext("TotalUncompressedSize", totalSizeOfArchive)
                                .ForContext("FilesToUploadUncompressedSize", totalSizeFilesToUpload)
                                .Information("Calculated uncompressed size of files to upload");

                            // Only create the folder structure if we didn't find any files uploaded
                            if (uploadResumeReport.SizeUploaded == 0)
                            {
                                CreateFolderStructure(gameName, archivePath);    
                            }
                            else
                            {
                                _notifier.SkipCreateFolderStructure(gameName);
                            }
                            
                            UploadAllFiles(gameName, uploadResumeReport);
                            
                            Log.ForContext("DurationMs",uploadDuration.ElapsedMilliseconds).Information("Upload finished");
                            _notifier.FinishedGameUpload(gameName, uploadDuration.Elapsed);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Unhandled exception occured while uploading archive.");
                            _notifier.GameUploadError(gameName, ex, $"An unhandled exception occurred while uploading archive for {gameName}");
                        }
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

        private IList<IZipEntry> GetAllFilesInArchive(string gameName, string archivePath)
        {
            using (IZipFile zip = _zipFileProcessor.Read(archivePath))
            {
                // Order files alphabetically to ensure we can resume interrupted uploads
                List<IZipEntry> files = zip.ReadAllFiles();
                return files;
            }
        }
        
        private UploadResumeReport GetFilesToUpload(string gameName, string archivePath)
        {
            IXboxGameRepository xboxGameRepository = _xboxGameRepositoryFactory.Create();
            xboxGameRepository.Connect();

            UploadResumeReport uploadResumeReport = null;
            
            using (IZipFile zip = _zipFileProcessor.Read(archivePath))
            {
                // Order files alphabetically to ensure we can resume interrupted uploads
                List<IZipEntry> files = zip.ReadAllFiles();
                
                _notifier.CheckingForUploadedFiles(gameName);
                
                IUploadResumeStrategy uploadResumeStrategy = new BinarySearchUploadResumeStrategy(files, _notifier, gameName, xboxGameRepository);

                uploadResumeReport = uploadResumeStrategy.GetRemainingFiles();
            }

            xboxGameRepository.Disconnect();

            return uploadResumeReport;
        }

        private void UploadAllFiles(string gameName, UploadResumeReport uploadResumeReport)
        {
            long totalSizeToUpload = uploadResumeReport.RemainingFiles.Sum(x => x.UncompressedSize);
            
            ReportTotalBytesToUpload(gameName, totalSizeToUpload);

            BlockingCollection<IXboxTransferRequest> finishedRequests = new BlockingCollection<IXboxTransferRequest>();

            XboxTransferWorker fileWorker1 = new XboxTransferWorker(_xboxGameRepositoryFactory, gameName, _xboxFtpRequests, finishedRequests,_notifier);
            XboxTransferWorker fileWorker2 = new XboxTransferWorker(_xboxGameRepositoryFactory, gameName, _xboxFtpRequests, finishedRequests, _notifier);
            XboxTransferProgressWorker progressWorker = new XboxTransferProgressWorker(_xboxGameRepositoryFactory, gameName,_notifier, finishedRequests,
                totalSizeToUpload, uploadResumeReport.SizeUploaded);

            progressWorker.Start();
            fileWorker1.Start();
            fileWorker2.Start();

            _notifier.ReportTotalFilesToTransfer(gameName, uploadResumeReport.RemainingFiles.Count);

            foreach (var zipEntry in uploadResumeReport.RemainingFiles)
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
            
            using (IZipFile zip = _zipFileProcessor.Read(archivePath))
            {
                var directories = zip.GetDirectories();

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
