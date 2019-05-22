using FluentFTP;

namespace Adapter.Persistence.Ftp
{
    public class FtpClientFactory
    {
        private readonly FtpXboxSettings _ftpXboxSettings;

        public FtpClientFactory(FtpXboxSettings ftpXboxSettings)
        {
            _ftpXboxSettings = ftpXboxSettings;
        }

        public FtpClient Create()
        {
            var ftpClient = new FtpClient(_ftpXboxSettings.Host, _ftpXboxSettings.Port, _ftpXboxSettings.User, _ftpXboxSettings.Password);
            ftpClient.ConnectTimeout = 2000;
            return ftpClient;
        }
    }
}