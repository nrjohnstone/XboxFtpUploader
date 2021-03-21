using FluentFTP;

namespace Adapter.Persistence.Ftp
{
    public interface IFtpClientFactory
    {
        IFtpClient Create();
    }
}