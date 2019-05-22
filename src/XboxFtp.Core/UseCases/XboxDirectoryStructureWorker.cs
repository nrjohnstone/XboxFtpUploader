using System;
using System.Collections.Concurrent;
using System.Threading;

namespace XboxFtp.Core.UseCases
{
    internal class XboxDirectoryStructureWorker : XboxWorkerBase
    {
        private readonly BlockingCollection<XboxDirectoryCreateRequest> _requests;

        public XboxDirectoryStructureWorker(IXboxGameRepositoryFactory xboxGameRepositoryFactory, string gameName, BlockingCollection<XboxDirectoryCreateRequest> requests) : base(xboxGameRepositoryFactory, gameName)
        {
            _requests = requests ?? throw new ArgumentNullException(nameof(requests));
        }

        protected override void OnStart()
        {
            XboxGameRepository.CreateGame(GameName);
        }

        protected override void ProcessNextRequest()
        {
            var request = _requests.TryTake(out var item, TimeSpan.FromMilliseconds(500));

            if (item == null)
                return;

            if (request)
            {
                XboxGameRepository.CreateDirectory(item.Path);
            }
        }
    }
}