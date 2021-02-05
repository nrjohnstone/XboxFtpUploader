using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using XboxFtp.Core.Entities;
using XboxFtp.Core.Ports.Notification;

namespace Adapter.Notifier.Serilog
{
    public class SerilogProgressNotifier : IProgressNotifier
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, ILogger> _gameLogger;

        public SerilogProgressNotifier(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;
            _gameLogger = new Dictionary<string, ILogger>();
        }

        public void GameAddedToUploadQueue(string gameName)
        {
            _logger.Information("{Game} added to upload queue", gameName);
        }

        public void StartingGameUpload(string gameName)
        {
            _logger.Information("Starting upload of {Game}", gameName);

            var gameLogger = CreateNewLogger(gameName);
            _gameLogger.Add(gameName, gameLogger);
        }

        public void FinishedGameUpload(string gameName, TimeSpan totalUploadTime)
        {
            _gameLogger[gameName].Information("Total upload time: {TotalUploadTime} second",
                totalUploadTime.TotalSeconds);
        }

        public void GameUploadError(string gameName, Exception ex, string errorMessage)
        {
            _gameLogger[gameName].Error(ex, errorMessage);
        }

        public void ReportTotalFilesToTransfer(string gameName, int count)
        {
            _gameLogger[gameName].Information("Uploading {FileToTransferCount} files", count);
        }

        public void ExtractFileToDisk(string gameName, string fileName)
        {
            _gameLogger[gameName].Debug("Extracting large file to disk: {FileName}", fileName);
        }

        public void AddingToUploadQueue(string gameName, string fileName)
        {
            _gameLogger[gameName].Debug("Requesting upload for {FileName}", fileName);
        }

        public void CreateFolderStructure(string gameName)
        {
            _gameLogger[gameName].Information("Creating folder structure for {Game}", gameName);
        }

        public void FinishedCreatingFolderStructure(string gameName)
        {
            _gameLogger[gameName].Information("All directories created");
        }

        public void CheckingForUploadedFiles(string gameName)
        {
            _gameLogger[gameName].Information("Checking for already uploaded files");
        }

        public void CheckingForUploadedFile(string gameName, string fileName)
        {
            _gameLogger[gameName].Debug("Checking for {FileName}", fileName);
        }

        public void FileAlreadyExists(string gameName, string fileName)
        {
            _gameLogger[gameName].Debug("File {FileName} already exists, skipping", fileName);
        }

        public void WaitingForUploadsToComplete(string gameName)
        {
            _gameLogger[gameName].Debug("Waiting for uploads to complete");
        }

        public void ReportTotalBytesToUpload(string gameName, long totalBytesToUpload)
        {
            _gameLogger[gameName].Information("Total bytes to upload: {TotalBytesToUpload}", totalBytesToUpload);
        }

        public void FinishedFileUpload(string gameName, IXboxTransferRequest item, int percentComplete)
        {
            _gameLogger[gameName].Information("File uploaded. Percent complete: {percentComplete}", percentComplete);
        }

        public void StartingFileUpload(string gameName, string fileName)
        {
            _gameLogger[gameName].Information("Starting file upload. {FileName} ", fileName);
        }

        private ILogger CreateNewLogger(string gameName)
        {
            string datePrefix = DateTime.Now.ToString("yyyyMMddhhmm");
            var reportName = $"{datePrefix}-{gameName}_v2.txt";
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "XboxFtp", reportName);

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(filePath)
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .CreateLogger();
            
            return logger;
        }
    }
}