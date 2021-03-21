using System;
using System.Runtime.Serialization;

namespace Adapter.Persistence.Ftp
{
    [Serializable]
    public class PersistenceException : Exception
    {
        private readonly bool _isTransient;

        public PersistenceException(bool isTransient)
        {
            _isTransient = isTransient;
        }

        public PersistenceException(string message, bool isTransient) : base(message)
        {
            _isTransient = isTransient;
        }

        public PersistenceException(string message, bool isTransient, Exception inner) : base(message, inner)
        {
            _isTransient = isTransient;
        }

        protected PersistenceException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public bool IsTransient => _isTransient;
    }
}