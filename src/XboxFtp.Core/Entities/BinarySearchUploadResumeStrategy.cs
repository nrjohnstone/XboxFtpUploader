using System;
using System.Collections.Generic;
using System.Linq;
using XboxFtp.Core.Ports.Notification;
using XboxFtp.Core.Ports.Persistence;

namespace XboxFtp.Core.Entities
{
    /// <summary>
    /// A resume strategy that uses a binary search to find the resume point
    /// </summary>
    public class BinarySearchUploadResumeStrategy : IUploadResumeStrategy
    {
        private readonly IList<IZipEntry> _filesToCheck;
        private readonly IProgressNotifier _notifier;
        private readonly string _gameName;
        private readonly IXboxGameRepository _xboxGameRepository;
        
        public BinarySearchUploadResumeStrategy(IList<IZipEntry> filesToCheck, IProgressNotifier notifier, string gameName, IXboxGameRepository xboxGameRepository)
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
            int lowerBound = 0;
            int upperBound = _filesToCheck.Count - 1;
            int index = upperBound / 2;
            int resumePosition = index;
            
            var filesToCheckList = _filesToCheck.ToList();
            
            bool resumePointFound = false;
            
            // Check the first file explicitly
            var zipEntry = filesToCheckList[0];
            if (!_xboxGameRepository.Exists(_gameName, zipEntry.FileName, zipEntry.UncompressedSize))
            {
                return filesToCheckList;
            }

            while (lowerBound <= upperBound)
            {
                zipEntry = filesToCheckList[index];
                
                _notifier.CheckingForUploadedFile(_gameName, zipEntry.FileName);
                    
                if (_xboxGameRepository.Exists(_gameName, zipEntry.FileName, zipEntry.UncompressedSize))
                {
                    resumePosition = index;
                    _notifier.FileAlreadyExists(_gameName, zipEntry.FileName);
                    lowerBound = index + 1;
                    index = (lowerBound +  upperBound) / 2;
                }
                else
                {
                    upperBound = index - 1;
                    index = (lowerBound +  upperBound) / 2;
                }
            }

            filesToCheckList.RemoveRange(0, resumePosition + 1);
            return filesToCheckList;
        }
    }
}