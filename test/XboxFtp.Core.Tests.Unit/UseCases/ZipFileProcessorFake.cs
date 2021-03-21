using XboxFtp.Core.UseCases;

namespace XboxFtp.Core.Tests.Unit.UseCases
{
    internal class ZipFileProcessorFake : IZipFileProcessor
    {
        private IZipFile _zipFile;

       
        public void SetZipFile(IZipFile zipFile)
        {
            _zipFile = zipFile;
        }

        public IZipFile Read(string archivePath)
        {
            return _zipFile;
        }
    }
}