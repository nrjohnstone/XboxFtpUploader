using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using XboxFtp.Core.Ports.Persistence;

namespace Adapter.Persistence.InMemory
{
    public class XboxGameRepositoryInMemory : IXboxGameRepository
    {
        private readonly Dictionary<string, long> _data;
        private readonly TimeSpan _fileUploadDelay;

        public XboxGameRepositoryInMemory(Dictionary<string, long> data, TimeSpan fileUploadDelay)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            _data = data;
            _fileUploadDelay = fileUploadDelay;
        }

        public void Connect()
        {
        }

        public void Disconnect()
        {
        }

        public void CreateGame(string gameName)
        {
        }

        public void Store(string gameName, string targetFilePath, byte[] data)
        {
            _data[$"{gameName}|{targetFilePath}"] = data.Length;
            Thread.Sleep(_fileUploadDelay);
        }

        public void Store(string gameName, string targetFilePath, Stream data)
        {
            _data[$"{gameName}|{targetFilePath}"] = data.Length;
            Thread.Sleep(_fileUploadDelay);
        }

        public bool Exists(string gameName, string targetFilePath, long size)
        {
            if (!_data.ContainsKey($"{gameName}|{targetFilePath}"))
                return false;

            if (_data[$"{gameName}|{targetFilePath}"] == size)
                return true;

            return false;
        }

        public void CreateDirectory(string targetDirectory)
        {
        }
    }
}
