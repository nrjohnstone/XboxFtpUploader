using System.IO;
using Ionic.Zip;

namespace XboxFtp.Core.Entities
{
    public interface IZipEntry
    {
        string FileName { get; }
        long UncompressedSize { get; }
        void Extract(string baseDirectory, ExtractExistingFileAction extractExistingFileAction);
        Stream OpenReader();
    }
}