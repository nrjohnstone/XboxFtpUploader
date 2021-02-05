using System;
using System.Threading;
using XboxFtp.Core.Ports.Persistence;

namespace XboxFtp.Core.UseCases
{
    internal class XboxWorkerBase : WorkerBase
    {
        protected IXboxGameRepository XboxGameRepository;
        protected readonly IXboxGameRepositoryFactory XboxGameRepositoryFactory;
        protected readonly string GameName;
        
        public XboxWorkerBase(IXboxGameRepositoryFactory xboxGameRepositoryFactory, string gameName)
        {
            XboxGameRepositoryFactory = xboxGameRepositoryFactory ?? throw new ArgumentNullException(nameof(xboxGameRepositoryFactory));
            GameName = gameName ?? throw new ArgumentNullException(nameof(gameName));
        }

        public override void Start()
        {
            XboxGameRepository = XboxGameRepositoryFactory.Create();
            XboxGameRepository.Connect();

            OnStart();

            base.Start();
        }

        public override void Stop()
        {
            base.Stop();

            WaitUntilInProgressTransfersComplete();

            XboxGameRepository.Disconnect();
        }

        private void WaitUntilInProgressTransfersComplete()
        {
            while (Processing)
            {
                Thread.Sleep(200);
            }
        }

        protected virtual void OnStart() { }
    }
}