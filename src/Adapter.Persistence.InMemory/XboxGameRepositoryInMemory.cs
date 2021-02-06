using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using XboxFtp.Core.Ports.Persistence;

namespace Adapter.Persistence.InMemory
{
    public class XboxGameRepositoryInMemory : IXboxGameRepository
    {
        private class XboxFileDto
        {
            public string FilePath { get; set; }
            public long Size { get; set; }
        }
        
        private readonly Dictionary<string, XboxFileDto> _data;
        public TimeSpan FileUploadDelay { get; set; }

        public XboxGameRepositoryInMemory(TimeSpan fileUploadDelay)
        {
            _data = new Dictionary<string, XboxFileDto>();
            FileUploadDelay = fileUploadDelay;
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
            _data[$"{gameName}|{targetFilePath}"] = new XboxFileDto()
            {
                FilePath = targetFilePath,
                Size = data.Length
            };
            
            Thread.Sleep(FileUploadDelay);
        }

        public void Store(string gameName, string targetFilePath, Stream data)
        {
            _data[$"{gameName}|{targetFilePath}"] = new XboxFileDto()
            {
                FilePath = targetFilePath,
                Size = data.Length
            };
            
            Thread.Sleep(FileUploadDelay);
        }

        public bool Exists(string gameName, string targetFilePath, long size)
        {
            if (!_data.ContainsKey($"{gameName}|{targetFilePath}"))
                return false;

            if (_data[$"{gameName}|{targetFilePath}"].Size == size)
                return true;

            return false;
        }

        public void CreateDirectory(string targetDirectory)
        {
        }
    }
}
