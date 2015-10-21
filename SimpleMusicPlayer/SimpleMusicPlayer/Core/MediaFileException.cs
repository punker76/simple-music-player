using System;
using System.Runtime.Serialization;

namespace SimpleMusicPlayer.Core
{
    [Serializable]
    public class MediaFileException : Exception
    {
        public MediaFileException()
        {
        }

        public MediaFileException(string message)
            : base(message)
        {
        }

        public MediaFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MediaFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}