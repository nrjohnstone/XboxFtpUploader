using XboxFtp.Core.Ports.Persistence;
using XboxFtp.Core.UseCases;

namespace Adapter.Persistence.Ftp
{
    public class FtpXboxGameRepositoryFactory : IXboxGameRepositoryFactory
    {
        private readonly FtpClientFactory _ftpClientFactory;
        private readonly FtpXboxSettings _ftpXboxSettings;

        public FtpXboxGameRepositoryFactory(FtpClientFactory ftpClientFactory, FtpXboxSettings ftpXboxSettings)
        {
            _ftpClientFactory = ftpClientFactory;
            _ftpXboxSettings = ftpXboxSettings;
        }

        public IXboxGameRepository Create()
        {
            return new FtpXboxGameRepository(_ftpClientFactory, _ftpXboxSettings);
        }
    }
}