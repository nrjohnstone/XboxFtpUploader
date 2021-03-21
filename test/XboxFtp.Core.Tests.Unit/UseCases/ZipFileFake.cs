using System.Collections.Generic;
using XboxFtp.Core.Entities;
using XboxFtp.Core.UseCases;

namespace XboxFtp.Core.Tests.Unit.UseCases
{
    internal class ZipFileFake : IZipFile
    {
        private List<IZipEntry> _zipEntries;

        public ZipFileFake(List<IZipEntry> zipEntries)
        {
            _zipEntries = zipEntries;
        }
     
        public void Dispose()
        {            
        }

        public List<IZipEntry> GetDirectories()
        {
            return null;
        }

        public List<IZipEntry> ReadAllFiles()
        {
            return _zipEntries;
        }
    }
}