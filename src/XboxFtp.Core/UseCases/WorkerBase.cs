using System;
using System.Threading;

namespace XboxFtp.Core.UseCases
{
    internal class WorkerBase
    {
        private Thread _workerThread;
        private bool _shutdown;
        protected bool Processing;

        public virtual void Start()
        {
            _workerThread = new Thread(Process);
            _workerThread.Start();
        }

        public virtual void Stop()
        {
            _shutdown = true;
            _workerThread.Join(TimeSpan.FromSeconds(2));
        }

        protected virtual void ProcessNextRequest() { }

        private void Process()
        {
            while (!_shutdown)
            {
                Processing = true;
                ProcessNextRequest();
            }

            Processing = false;
        }
    }
}