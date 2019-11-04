using System.IO;

namespace XboxFtp.Core.UseCases
{
    internal class XboxTempFileTransferRequest : IXboxTransferRequest
    {
        public string Path { get; set; }
        public long Length => new FileInfo(TempFilePath).Length;
        public string TempFilePath { get; set; }

        public byte[] GetData()
        {
            return File.ReadAllBytes(TempFilePath);
        }

        public Stream GetStream()
        {
            return File.Open(TempFilePath, FileMode.Open);
        }
    }
}