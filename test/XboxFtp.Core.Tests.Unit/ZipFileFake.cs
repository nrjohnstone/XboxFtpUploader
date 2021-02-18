using System.IO;
using Ionic.Zip;
using XboxFtp.Core.Entities;

namespace XboxFtp.Core.Tests.Unit
{
    internal class ZipFileFake : IZipEntry
    {
        public string FileName { get; set; }
        public long UncompressedSize { get; set; }
        
        public void Extract(string baseDirectory, ExtractExistingFileAction extractExistingFileAction)
        {
        }

        public Stream OpenReader()
        {
            return new MemoryStream();
        }
    }
}