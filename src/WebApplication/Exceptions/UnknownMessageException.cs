using System;

namespace WebApplication.Exceptions
{
    public class UnknownMessageException : Exception
    {
        public UnknownMessageException(string message) : base(message)
        { }

        public UnknownMessageException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
