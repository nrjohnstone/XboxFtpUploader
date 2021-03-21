using System;
using System.Collections.Concurrent;
using System.Threading;
using XboxFtp.Core.Entities;

namespace XboxFtp.Core.UseCases
{
    /// <summary>
    /// Creates the directory structure of an XBOX game before files are uploaded to remove any
    /// contention around separate workers creating the same directory 
    /// </summary>
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