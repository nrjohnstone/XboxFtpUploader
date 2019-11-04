using System;
using System.Collections.Concurrent;
using Serilog;

namespace XboxFtp.Core.UseCases
{
    internal class XboxTransferWorker : XboxWorkerBase
    {
        private readonly BlockingCollection<IXboxTransferRequest> _requests;
     
        public XboxTransferWorker(IXboxGameRepositoryFactory xboxGameRepositoryFactory, string gameName, BlockingCollection<IXboxTransferRequest> requests) : base(xboxGameRepositoryFactory, gameName)
        {
            _requests = requests ?? throw new ArgumentNullException(nameof(requests));
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
                    return;
                }
            }

            using (var stream = item.GetStream())
            {
                XboxGameRepository.Store(GameName, item.Path, stream);
            }
        }
    }
}