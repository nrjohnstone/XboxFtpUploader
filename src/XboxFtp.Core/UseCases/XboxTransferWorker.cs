using System;
using System.Collections.Concurrent;
using Serilog;
using XboxFtp.Core.Ports.Notification;

namespace XboxFtp.Core.UseCases
{
    internal class XboxTransferWorker : XboxWorkerBase
    {
        private readonly BlockingCollection<IXboxTransferRequest> _requests;
        private readonly BlockingCollection<IXboxTransferRequest> _finishedRequests;
        private readonly IProgressNotifier _notifier;

        public XboxTransferWorker(IXboxGameRepositoryFactory xboxGameRepositoryFactory, string gameName,
            BlockingCollection<IXboxTransferRequest> requests, BlockingCollection<IXboxTransferRequest> finishedRequests, IProgressNotifier notifier) : base(xboxGameRepositoryFactory, gameName)
        {
            _requests = requests ?? throw new ArgumentNullException(nameof(requests));
            _finishedRequests = finishedRequests;
            _notifier = notifier;
        }

        protected override void ProcessNextRequest()
        {
            IXboxTransferRequest item;
            var request = _requests.TryTake(out item, TimeSpan.FromSeconds(1));

            if (item == null)
                return;

            if (!request)
                return;

            if (item.Length > 50000)
            {
                if (XboxGameRepository.Exists(GameName, item.Path, item.Length))
                {
                    Log.Information("File already exists: Skipping");
                    _finishedRequests.Add(item);
                    return;
                }
            }

            using (var stream = item.GetStream())
            {
                _notifier.StartingFileUpload(GameName, item.Path);
                XboxGameRepository.Store(GameName, item.Path, stream);
                _finishedRequests.Add(item);
            }
        }
    }
}