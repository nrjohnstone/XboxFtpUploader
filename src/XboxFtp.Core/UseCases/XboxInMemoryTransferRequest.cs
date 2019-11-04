using System.IO;

namespace XboxFtp.Core.UseCases
{
    public class XboxInMemoryTransferRequest : IXboxTransferRequest
    {
        public string Path { get; set; }

        public long Length => Data.Length;

        public byte[] GetData()
        {
            return Data;
        }

        public Stream GetStream() { return new MemoryStream(Data);}

        public byte[] Data { get; set; }
    }

    public interface IXboxTransferRequest
    {
        string Path { get; }
        long Length { get; }
        byte[] GetData();
        Stream GetStream();
    }
}