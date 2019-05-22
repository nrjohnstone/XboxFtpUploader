using System;
using System.Threading;
using XboxFtp.Core.Ports.Persistence;

namespace XboxFtp.Core.UseCases
{
    internal class XboxWorkerBase
    {
        protected IXboxGameRepository XboxGameRepository;
        protected readonly IXboxGameRepositoryFactory XboxGameRepositoryFactory;
        protected readonly string GameName;
        protected Thread WorkerThread;
        protected bool Shutdown;
        protected bool Processing;


        public XboxWorkerBase(IXboxGameRepositoryFactory xboxGameRepositoryFactory, string gameName)
        {
            XboxGameRepositoryFactory = xboxGameRepositoryFactory ?? throw new ArgumentNullException(nameof(xboxGameRepositoryFactory));
            GameName = gameName ?? throw new ArgumentNullException(nameof(gameName));
        }

        public void Start()
        {
            XboxGameRepository = XboxGameRepositoryFactory.Create();
            XboxGameRepository.Connect();

            OnStart();

            WorkerThread = new Thread(ProcessQueue);
            WorkerThread.Start();
        }

        public void Stop()
        {
            Shutdown = true;
            
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

        private void ProcessQueue()
        {
            while (!Shutdown)
            {
                Processing = true;
                ProcessNextRequest();
            }

            Processing = false;
        }

        protected virtual void ProcessNextRequest() { }

    }
}