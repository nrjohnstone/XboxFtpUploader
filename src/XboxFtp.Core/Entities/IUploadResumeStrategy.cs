using System.Collections.Generic;

namespace XboxFtp.Core.Entities
{
    public interface IUploadResumeStrategy
    {
        IList<IZipEntry> GetRemainingFiles();
    }
}