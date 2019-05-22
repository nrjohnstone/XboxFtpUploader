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
                    Stopwatch uploadDuration = Stopwatch.StartNew();
                    string gameName = GetGameNameFromPath(archivePath);
                    var filesToUpload = GetFilesToUpload(gameName, archivePath);

                    CreateFolderStructure(gameName, archivePath);
                    UploadAllFiles(gameName, filesToUpload);

                    Log.Information("Total upload time: {TotalUploadTime} second",
                        uploadDuration.Elapsed.TotalSeconds);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unhandled exception occurred while uploading archives");
            }
        }

        private static string GetGameNameFromPath(string archivePath)
        {
            return Path.GetFileNameWithoutExtension(archivePath);
        }

        private List<ZipEntry> GetFilesToUpload(string gameName, string archivePath)
        {
            IXboxGameRepository xboxGameRepository = _xboxGameRepositoryFactory.Create();
            xboxGameRepository.Connect();
            
            Queue<ZipEntry> files;

            using (ZipFile zip = ZipFile.Read(archivePath))
            {
                // Order files alphabetically
                files = new Queue<ZipEntry>(zip.Where(entry => !entry.IsDirectory).OrderBy(entry => entry.FileName));

                Log.Information("Checking for already uploaded files");

                // Find the first file that does not exist on the xbox and resume uploading from that file
                while (files.Count > 0)
                {
                    var zipEntry = files.Peek();

                    Log.Debug("Checking for {FileName}", zipEntry.FileName);

                    if (xboxGameRepository.Exists(gameName, zipEntry.FileName, zipEntry.UncompressedSize))
                    {
                        files.Dequeue();
                        Log.Debug("File {FileName} already exists, skipping", zipEntry.FileName);
                        continue;
                    }

                    break;
                }
            }

            xboxGameRepository.Disconnect();

            return files.ToList();
        }

        private void UploadAllFiles(string gameName, List<ZipEntry> filesToUpload)
        {
            XboxTransferWorker fileWorker1 = new XboxTransferWorker(_xboxGameRepositoryFactory, gameName, _xboxFtpRequests);
            XboxTransferWorker fileWorker2 = new XboxTransferWorker(_xboxGameRepositoryFactory, gameName, _xboxFtpRequests);

            fileWorker1.Start();
            fileWorker2.Start();

            Log.Information("Uploading {FileToTransferCount} files", filesToUpload.Count);
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
                    Log.Debug("Requesting upload for {FileName}", zipEntry.FileName);
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

        private void CreateFolderStructure(string gameName, string archivePath)
        {
            Log.Information("Creating folder structure for {Game}", gameName);

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
            Log.Information("Stopping folderWorker");
            folderWorker.Stop();
            Log.Information("Stopped folderWorker");
        }
    }

    public class XboxTransferRequest
    {
        public string Path { get; set; }
        public byte[] Data { get; set; }
    }

    public class XboxDirectoryCreateRequest
    {
        public string Path { get; set; }
    }
}
