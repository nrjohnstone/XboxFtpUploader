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
        private long _totalBytesUploaded;

        public XboxTransferProgressWorker(IXboxGameRepositoryFactory xboxGameRepositoryFactory, string gameName,
            IProgressNotifier notifier, BlockingCollection<IXboxTransferRequest> finishedRequests, long totalBytesToUpload)
        {
            _gameName = gameName;
            _notifier = notifier;
            _finishedRequests = finishedRequests;
            _totalBytesToUpload = totalBytesToUpload;
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

            int percentComplete = (int) (((float) _totalBytesUploaded / (float) _totalBytesToUpload) * 100);

            _notifier.FinishedFileUpload(_gameName, item, percentComplete);
        }
    }
}