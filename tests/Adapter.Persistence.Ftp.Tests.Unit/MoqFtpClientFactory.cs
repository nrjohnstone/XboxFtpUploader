using FluentFTP;
using Moq;

namespace Adapter.Persistence.Ftp.Tests.Unit
{
    internal class MoqFtpClientFactory : IFtpClientFactory
    {
        private readonly Mock<IFtpClient> _mockFtpClient;

        public MoqFtpClientFactory(Mock<IFtpClient> mockFtpClient)
        {
            _mockFtpClient = mockFtpClient;
        }
        
        public IFtpClient Create()
        {
            return _mockFtpClient.Object;
        }
    }
}