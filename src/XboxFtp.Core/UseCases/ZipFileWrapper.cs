using System.Collections.Generic;
using System.Linq;
using Ionic.Zip;
using XboxFtp.Core.Entities;

namespace XboxFtp.Core.UseCases
{
    public class ZipFileWrapper : IZipFile
    {
        private readonly ZipFile _zipFile;

        public ZipFileWrapper(ZipFile zipFile)
        {
            _zipFile = zipFile;
        }

        public void Dispose()
        {
            _zipFile.Dispose();
        }

        public List<IZipEntry> GetDirectories()
        {
            var y = _zipFile.Where(entry => entry.IsDirectory).Select(x => new ZipEntryWrapper(x)).Cast<IZipEntry>().ToList();
            return y;
        }

        public List<IZipEntry> ReadAllFiles()
        {
            List<IZipEntry> y = _zipFile.Where(entry => !entry.IsDirectory).Select(x => new ZipEntryWrapper(x)).OrderBy(entry => entry.FileName).Cast<IZipEntry>().ToList();
            return y;
        }

    }
}