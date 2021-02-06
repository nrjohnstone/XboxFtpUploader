using System;
using System.IO;
using Ionic.Zip;

namespace XboxFtp.Core.Entities
{
    /// <summary>
    /// Simple wrapper for the ZipEntry type so it can be more
    /// easily tested
    /// </summary>
    public class ZipEntryWrapper : IZipEntry
    {
        private readonly ZipEntry _zipEntry;

        public ZipEntryWrapper(ZipEntry zipEntry)
        {
            if (zipEntry == null) throw new ArgumentNullException(nameof(zipEntry));
            _zipEntry = zipEntry;
        }

        public string FileName => _zipEntry.FileName;
        public long UncompressedSize => _zipEntry.UncompressedSize;
        
        public void Extract(string baseDirectory, ExtractExistingFileAction extractExistingFileAction)
        {
            _zipEntry.Extract(baseDirectory, extractExistingFileAction);
        }

        public Stream OpenReader()
        {
            return _zipEntry.OpenReader();
        }
    }
}