using System;
using System.Collections.Generic;
using System.Linq;
using XboxFtp.Core.Ports.Notification;
using XboxFtp.Core.Ports.Persistence;

namespace XboxFtp.Core.Entities
{
    /// <summary>
    /// A resume strategy for uploading files that checks the files sequentially
    /// and compares the size to see if they are already present in the IXboxGameRepository
    /// </summary>
    public class SequentialUploadResumeStrategy : IUploadResumeStrategy
    {
        private readonly IList<IZipEntry> _filesToCheck;
        private readonly IProgressNotifier _notifier;
        private readonly string _gameName;
        private readonly IXboxGameRepository _xboxGameRepository;

        public SequentialUploadResumeStrategy(IList<IZipEntry> filesToCheck, IProgressNotifier notifier, string gameName, IXboxGameRepository xboxGameRepository)
        {
            if (filesToCheck == null) throw new ArgumentNullException(nameof(filesToCheck));
            if (notifier == null) throw new ArgumentNullException(nameof(notifier));
            if (gameName == null) throw new ArgumentNullException(nameof(gameName));
            if (xboxGameRepository == null) throw new ArgumentNullException(nameof(xboxGameRepository));
            _filesToCheck = filesToCheck;
            _notifier = notifier;
            _gameName = gameName;
            _xboxGameRepository = xboxGameRepository;
        }

        public IList<IZipEntry> GetRemainingFiles()
        {
            var filesToCheck = _filesToCheck.ToList();
            int filesExistCount = 0;
            
            for (int i = 0; i < filesToCheck.Count(); i++)
            {
                IZipEntry zipEntry = filesToCheck[i];
                
                _notifier.CheckingForUploadedFile(_gameName, zipEntry.FileName);
                    
                if (!_xboxGameRepository.Exists(_gameName, zipEntry.FileName, zipEntry.UncompressedSize))
                {
                    break;
                }
                
                filesExistCount++;
                _notifier.FileAlreadyExists(_gameName, zipEntry.FileName);
            }
            
            filesToCheck.RemoveRange(0, filesExistCount);

            return filesToCheck;
        }
    }
}