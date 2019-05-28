using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Crc;
using Ionic.Zip;
using Serilog;
using XboxFtp.Core.Ports.Persistence;

namespace XboxFtp.Core.UseCases
{
    public interface IXboxGameRepositoryFactory
    {
        IXboxGameRepository Create();
    }

    public class UploadArchivesUseCase
    {
        private readonly IXboxGameRepositoryFactory _xboxGameRepositoryFactory;
        private readonly BlockingCollection<XboxTransferRequest> _xboxFtpRequests;
        private readonly BlockingCollection<XboxDirectoryCreateRequest> _xboxDirectoryCreateRequests;

        public UploadArchivesUseCase(IXboxGameRepositoryFactory xboxGameRepositoryFactory)
        {
            _xboxGameRepositoryFactory = xboxGameRepositoryFactory;
            _xboxFtpRequests = new BlockingCollection<XboxTransferRequest>();
            _xboxDirectoryCreateRequests = new BlockingCollection<XboxDirectoryCreateRequest>();
        }

        public void Execute(List<string> archivePaths)
        {
            try
            {
                foreach (var archivePath in archivePaths)
                {
                    string gameName = GetGameNameFromPath(archivePath);
                    ILogger reportLogger = CreateNewLogger(gameName);

                    try
                    {
                        Stopwatch uploadDuration = Stopwatch.StartNew();
                        var filesToUpload = GetFilesToUpload(gameName, archivePath, reportLogger);

                        CreateFolderStructure(gameName, archivePath, reportLogger);
                        UploadAllFiles(gameName, filesToUpload, reportLogger);

                        reportLogger.Information("Total upload time: {TotalUploadTime} second",
                            uploadDuration.Elapsed.TotalSeconds);
                    }
                    catch (Exception ex)
                    {
                        reportLogger.Error(ex, $"An unhandled exception occurred while uploading archive for {gameName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unhandled exception occurred while uploading archives");
            }
        }

        private ILogger CreateNewLogger(string gameName)
        {
            string datePrefix = DateTime.Now.ToString("yyyyMMddhhmm");
            var reportName = $"{datePrefix}-{gameName}.txt";
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "XboxFtp", reportName);
            
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(filePath)
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .CreateLogger();
            return logger;
        }

        private static string GetGameNameFromPath(string archivePath)
        {
            return Path.GetFileNameWithoutExtension(archivePath);
        }

        private List<ZipEntry> GetFilesToUpload(string gameName, string archivePath, ILogger logger)
        {
            IXboxGameRepository xboxGameRepository = _xboxGameRepositoryFactory.Create();
            xboxGameRepository.Connect();
            
            Queue<ZipEntry> files;

            using (ZipFile zip = ZipFile.Read(archivePath))
            {
                // Order files alphabetically
                files = new Queue<ZipEntry>(zip.Where(entry => !entry.IsDirectory).OrderBy(entry => entry.FileName));

                logger.Information("Checking for already uploaded files");

                // Find the first file that does not exist on the xbox and resume uploading from that file
                while (files.Count > 0)
                {
                    var zipEntry = files.Peek();

                    logger.Debug("Checking for {FileName}", zipEntry.FileName);

                    if (xboxGameRepository.Exists(gameName, zipEntry.FileName, zipEntry.UncompressedSize))
                    {
                        files.Dequeue();
                        logger.Debug("File {FileName} already exists, skipping", zipEntry.FileName);
                        continue;
                    }

                    break;
                }
            }

            xboxGameRepository.Disconnect();

            return files.ToList();
        }

        private void UploadAllFiles(string gameName, List<ZipEntry> filesToUpload, ILogger logger)
        {
            XboxTransferWorker fileWorker1 = new XboxTransferWorker(_xboxGameRepositoryFactory, gameName, _xboxFtpRequests);
            XboxTransferWorker fileWorker2 = new XboxTransferWorker(_xboxGameRepositoryFactory, gameName, _xboxFtpRequests);

            fileWorker1.Start();
            fileWorker2.Start();

            logger.Information("Uploading {FileToTransferCount} files", filesToUpload.Count);
            foreach (var zipEntry in filesToUpload)
            {
                using (CrcCalculatorStream reader = zipEntry.OpenReader())
                {
                    WaitIfMaxOutstandingRequests();

                    byte[] data;
                    data = new byte[zipEntry.UncompressedSize];
                    reader.Read(data, 0, (int)zipEntry.UncompressedSize);

                    XboxTransferRequest request = new XboxTransferRequest()
                    {
                        Path = zipEntry.FileName,
                        Data = data
                    };
                    reader.Close();
                    logger.Debug("Requesting upload for {FileName}", zipEntry.FileName);
                    _xboxFtpRequests.Add(request);
                }
            }

            WaitForRequestsToComplete();
            fileWorker1.Stop();
            fileWorker2.Stop();
        }

        private void WaitIfMaxOutstandingRequests()
        {
            while (_xboxFtpRequests.Count >= 10)
            {
                Thread.Sleep(500);
            }
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
            Log.Information("All directories created");
        }

        private void CreateFolderStructure(string gameName, string archivePath, ILogger logger)
        {
            logger.Information("Creating folder structure for {Game}", gameName);

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
            logger.Information("Stopping folderWorker");
            folderWorker.Stop();
            logger.Information("Stopped folderWorker");
        }
    }
}
