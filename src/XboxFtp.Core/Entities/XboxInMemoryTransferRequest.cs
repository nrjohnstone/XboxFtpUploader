using System.IO;

namespace XboxFtp.Core.Entities
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
}