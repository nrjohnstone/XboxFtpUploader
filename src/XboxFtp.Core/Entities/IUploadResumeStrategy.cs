using System.Collections.Generic;
using Ionic.Zip;
using XboxFtp.Core.Entities;

namespace XboxFtp.Core.UseCases
{
    public interface IUploadResumeStrategy
    {
        IList<IZipEntry> GetRemainingFiles();
    }
}