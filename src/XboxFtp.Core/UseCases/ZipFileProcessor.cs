using Ionic.Zip;

namespace XboxFtp.Core.UseCases
{
    public class ZipFileProcessor : IZipFileProcessor
    {
        public IZipFile Read(string archivePath)
        {
            ZipFile zipFile = ZipFile.Read(archivePath);
            return new ZipFileWrapper(zipFile);
        }
    }
}