using System;
using System.IO;
using System.Net;
using System.Threading;
using FluentFTP;
using XboxFtp.Core.Ports.Persistence;

namespace Adapter.Persistence.Ftp
{
    public class FtpXboxGameRepository : IXboxGameRepository
    {
        private readonly FtpClientFactory _ftpClientFactory;
        private FtpClient _ftpClient;
        private readonly FtpXboxSettings _ftpXboxSettings;
        private string _currentWorkingDirectory;

        public FtpXboxGameRepository(FtpClientFactory ftpClientFactory, FtpXboxSettings ftpXboxSettings)
        {
            if (ftpClientFactory == null) throw new ArgumentNullException(nameof(ftpClientFactory));
            if (ftpXboxSettings == null) throw new ArgumentNullException(nameof(ftpXboxSettings));
            _ftpClientFactory = ftpClientFactory;
            _ftpXboxSettings = ftpXboxSettings;
            _currentWorkingDirectory = "";
        }

        public void Connect()
        {
            DateTime retryMaxTime = DateTime.Now + TimeSpan.FromSeconds(30);

            _ftpClient = _ftpClientFactory.Create();

            while (DateTime.Now < retryMaxTime)
            {
                try
                {
                    _ftpClient.Connect();
                    break;
                }
                catch (FtpCommandException ex)
                {
                    Serilog.Log.Warning(ex, "Unable to connect. Retrying");
                    Thread.Sleep(2000);
                }
            }

            if (!_ftpClient.IsConnected)
                throw new InvalidOperationException("Unable to connect to FTP. Maximum retries exceeded");
        }

        public void Disconnect()
        {
            _ftpClient.Disconnect();
        }

        public void CreateGame(string gameName)
        {
            ValidateFtpClient();

            _ftpClient.SetWorkingDirectory(_ftpXboxSettings.GameRootDirectory);
            string gameroot = Path.Combine(_ftpXboxSettings.GameRootDirectory, gameName);
            _ftpClient.CreateDirectory(gameroot);
            _ftpClient.SetWorkingDirectory(gameroot);
        }

        private void ValidateFtpClient()
        {
            if (_ftpClient == null)
                throw new InvalidOperationException("Must call connect before calling other operations");
        }

        public void Store(string gameName, string targetFilePath, byte[] data)
        {
            ValidateFtpClient();
            
            string gameRoot = Path.Combine(_ftpXboxSettings.GameRootDirectory, gameName);
            SetWorkingDirectory(gameRoot);

            using (Stream stream = new MemoryStream(data))
            {
                _ftpClient.Upload(stream, targetFilePath);
            }
        }

        public bool Exists(string gameName, string targetFilePath, long size)
        {
            ValidateFtpClient();

            string gameRoot = Path.Combine(_ftpXboxSettings.GameRootDirectory, gameName);
            string path = Path.Combine(gameRoot, targetFilePath);

            // For FileExist or GetObjectInfo to work, the current working directory must be the drive
            // where the file exists
            SetWorkingDirectory(_ftpXboxSettings.GameRootDirectory);
            
            // Due to a bug in the FluentFtp library, must convert \\ from Path.Combine to /
            string newPath = ConvertPath(path);

            try
            {
                FtpListItem fileInfo = _ftpClient.GetObjectInfo(newPath);

                if (fileInfo == null)
                    return false;

                if (fileInfo.Size == size)
                    return true;
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }

        private void SetWorkingDirectory(string directory)
        {
            if (!_currentWorkingDirectory.Equals(directory))
            {
                _ftpClient.SetWorkingDirectory(directory);
                _currentWorkingDirectory = directory;
            }
        }

        private string ConvertPath(string path)
        {
            return path.Replace("\\", "/");
        }

        public void CreateDirectory(string targetDirectory)
        {
            ValidateFtpClient();

            _ftpClient.CreateDirectory(targetDirectory);
        }
    }
}
