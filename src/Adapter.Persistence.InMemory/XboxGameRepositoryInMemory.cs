using System;
using System.Collections.Generic;
using System.Threading;
using XboxFtp.Core.Ports.Persistence;

namespace Adapter.Persistence.InMemory
{
    public class XboxGameRepositoryInMemory : IXboxGameRepository
    {
        private readonly Dictionary<string, long> _data;

        public XboxGameRepositoryInMemory(Dictionary<string, long> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            _data = data;
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
