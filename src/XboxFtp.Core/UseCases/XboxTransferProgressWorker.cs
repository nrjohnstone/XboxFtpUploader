using System;
using System.Collections.Concurrent;
using XboxFtp.Core.Entities;
using XboxFtp.Core.Ports.Notification;

namespace XboxFtp.Core.UseCases
{
    internal class XboxTransferProgressWorker : WorkerBase
    {
        private readonly string _gameName;
        private readonly IProgressNotifier _notifier;
        private readonly BlockingCollection<IXboxTransferRequest> _finishedRequests;
        private readonly long _totalBytesToUpload;
        private readonly long _totalBytesAlreadyUploaded;
        private long _totalBytesUploaded;

        public XboxTransferProgressWorker(IXboxGameRepositoryFactory xboxGameRepositoryFactory, string gameName,
            IProgressNotifier notifier, BlockingCollection<IXboxTransferRequest> finishedRequests, long totalBytesToUpload, long totalBytesAlreadyUploaded)
        {
            _gameName = gameName;
            _notifier = notifier;
            _finishedRequests = finishedRequests;
            _totalBytesToUpload = totalBytesToUpload;
            _totalBytesAlreadyUploaded = totalBytesAlreadyUploaded;
        }

        protected override void ProcessNextRequest()
        {
            IXboxTransferRequest item;
            var request = _finishedRequests.TryTake(out item, TimeSpan.FromMilliseconds(500));

            if (item == null)
                return;

            if (!request)
                return;

            _totalBytesUploaded += item.Length;

            long totalBytesForGame = _totalBytesAlreadyUploaded + _totalBytesToUpload;

            int percentComplete = (int) (((float) (_totalBytesAlreadyUploaded + _totalBytesUploaded) / (float) totalBytesForGame) * 100);

            _notifier.FinishedFileUpload(_gameName, item, percentComplete);
        }
    }
}