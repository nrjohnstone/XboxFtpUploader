using FluentFTP;
using Serilog;

namespace Adapter.Persistence.Ftp
{
    public class FtpClientFactory : IFtpClientFactory
    {
        private readonly FtpXboxSettings _ftpXboxSettings;

        public FtpClientFactory(FtpXboxSettings ftpXboxSettings)
        {
            _ftpXboxSettings = ftpXboxSettings;
        }

        public IFtpClient Create()
        {
            var ftpClient = new FtpClient(_ftpXboxSettings.Host, _ftpXboxSettings.Port, _ftpXboxSettings.User, _ftpXboxSettings.Password);
            ftpClient.ConnectTimeout = 2000;
            Log.Debug("FtpClient created");
            return ftpClient;
        }
    }
}