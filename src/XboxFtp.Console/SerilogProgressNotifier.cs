using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using XboxFtp.Core.Ports.Notification;

namespace XboxFtp.Console
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
            _logger.Information("{gameName} added to upload queue", gameName);
        }

        public void StartingGameUpload(string gameName)
        {
            _logger.Information("Starting upload of {gameName}", gameName);

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