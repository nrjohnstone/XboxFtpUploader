using System;
using System.Collections.Generic;
using System.Linq;
using Ionic.Zip;
using XboxFtp.Core.Entities;
using XboxFtp.Core.Ports.Notification;
using XboxFtp.Core.Ports.Persistence;

namespace XboxFtp.Core.UseCases
{
    /// <summary>
    /// A resume strategy for uploading files that checks the files sequentially
    /// and compares the size to see if they are already present in the IXboxGameRepository
    /// </summary>
    public class SequentialUploadResumeStrategy : IUploadResumeStrategy
    {
        private readonly Queue<IZipEntry> _filesToCheck;
        private readonly IProgressNotifier _notifier;
        private readonly string _gameName;
        private readonly IXboxGameRepository _xboxGameRepository;

        public SequentialUploadResumeStrategy(Queue<IZipEntry> filesToCheck, IProgressNotifier notifier, string gameName, IXboxGameRepository xboxGameRepository)
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
            // Find the first file that does not exist on the xbox with the same size and resume uploading from that file
            while (_filesToCheck.Count > 0)
            {
                IZipEntry zipEntry = _filesToCheck.Peek();
                    
                _notifier.CheckingForUploadedFile(_gameName, zipEntry.FileName);
                    
                if (_xboxGameRepository.Exists(_gameName, zipEntry.FileName, zipEntry.UncompressedSize))
                {
                    _filesToCheck.Dequeue();
                    _notifier.FileAlreadyExists(_gameName, zipEntry.FileName);
                    continue;
                }

                break;
            }

            return _filesToCheck.ToList();
        }
    }
}