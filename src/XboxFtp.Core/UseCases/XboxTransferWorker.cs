using System;
using System.Collections.Concurrent;
using Serilog;

namespace XboxFtp.Core.UseCases
{
    internal class XboxTransferWorker : XboxWorkerBase
    {
        private readonly BlockingCollection<XboxTransferRequest> _requests;
     
        public XboxTransferWorker(IXboxGameRepositoryFactory xboxGameRepositoryFactory, string gameName, BlockingCollection<XboxTransferRequest> requests) : base(xboxGameRepositoryFactory, gameName)
        {
            _requests = requests ?? throw new ArgumentNullException(nameof(requests));
        }

        protected override void ProcessNextRequest()
        {
            XboxTransferRequest item;
            var request = _requests.TryTake(out item, TimeSpan.FromSeconds(1));

            if (item == null)
                return;

            if (!request)
                return;

            if (item.Data.Length > 50000)
            {
                if (XboxGameRepository.Exists(GameName, item.Path, item.Data.Length))
                {
                    Log.Information("File already exists: Skipping");
                    return;
                }
            }

            XboxGameRepository.Store(GameName, item.Path, item.Data);
        }
    }
}