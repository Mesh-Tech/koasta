using System;

namespace Koasta.Shared.Exceptions
{
    public class InvalidKeyException : Exception
    {
        public InvalidKeyException(string error) : base(error)
        {
        }

        public InvalidKeyException() : base()
        {
        }

        public InvalidKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
