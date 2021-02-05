using System.IO;

namespace XboxFtp.Core.Entities
{
    public interface IXboxTransferRequest
    {
        string Path { get; }
        long Length { get; }
        byte[] GetData();
        Stream GetStream();
    }
}