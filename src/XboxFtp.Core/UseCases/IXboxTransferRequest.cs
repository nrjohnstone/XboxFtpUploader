using System.IO;

namespace XboxFtp.Core.UseCases
{
    public interface IXboxTransferRequest
    {
        string Path { get; }
        long Length { get; }
        byte[] GetData();
        Stream GetStream();
    }
}