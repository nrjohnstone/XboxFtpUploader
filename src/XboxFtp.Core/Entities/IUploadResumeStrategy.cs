using System.Collections.Generic;

namespace XboxFtp.Core.Entities
{
    public interface IUploadResumeStrategy
    {
        UploadResumeReport GetRemainingFiles();
    }
}