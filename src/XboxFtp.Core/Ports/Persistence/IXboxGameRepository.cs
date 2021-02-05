using System;
using System.IO;
using System.Runtime.Serialization;

namespace XboxFtp.Core.Ports.Persistence
{
    public interface IXboxGameRepository
    {
        void Connect();
        void Disconnect();
        void CreateGame(string gameName);
        void Store(string gameName, string targetFilePath, byte[] data);
        void Store(string gameName, string targetFilePath, Stream data);
        bool Exists(string gameName, string targetFilePath, long size);
        void CreateDirectory(string targetDirectory);
    }

    [Serializable]
    public class PersistenceException : Exception
    {
        public PersistenceException()
        {
        }

        public PersistenceException(string message) : base(message)
        {
        }

        public PersistenceException(string message, Exception inner) : base(message, inner)
        {
        }

        protected PersistenceException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}