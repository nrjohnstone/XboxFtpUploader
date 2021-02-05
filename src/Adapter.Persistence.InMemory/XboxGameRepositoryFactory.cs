using System;
using System.Collections.Generic;
using XboxFtp.Core.Ports.Persistence;
using XboxFtp.Core.UseCases;

namespace Adapter.Persistence.InMemory
{
    public class XboxGameRepositoryFactory : IXboxGameRepositoryFactory
    {
        private readonly Dictionary<string, long> _data;
        private TimeSpan _fileUploadDelay;

        public XboxGameRepositoryFactory(Dictionary<string, long> data, TimeSpan fileUploadDelay)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            _data = data;
            _fileUploadDelay = fileUploadDelay;
        }

        public IXboxGameRepository Create()
        {
            return new XboxGameRepositoryInMemory(_data, _fileUploadDelay);
        }
    }
}