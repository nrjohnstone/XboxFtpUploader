using System.IO;
using Ionic.Crc;
using Ionic.Zip;

namespace XboxFtp.Core.UseCases
{
    public class XboxZipStreamTransferRequest : IXboxTransferRequest
    {
        public string Path { get; set; }
        public long Length => ZipEntry.UncompressedSize;

        public ZipEntry ZipEntry { get; set; }

        public byte[] GetData()
        {
            using (CrcCalculatorStream reader = ZipEntry.OpenReader())
            {
                byte[] data;
                data = new byte[ZipEntry.UncompressedSize];
                reader.Read(data, 0, (int) ZipEntry.UncompressedSize);
                return data;
            }
        }

        public Stream GetStream()
        {
            return ZipEntry.OpenReader();
        }
    }
}