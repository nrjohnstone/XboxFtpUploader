using System;
using System.Collections.Generic;
using XboxFtp.Core.Entities;

namespace XboxFtp.Core.UseCases
{
    public interface IZipFile : IDisposable
    {
        List<IZipEntry> GetDirectories();
        List<IZipEntry> ReadAllFiles();
    }
}