using System;

namespace WebApplication.Exceptions
{
    public class CreateUserException : Exception
    {
        public CreateUserException(string message) : base(message)
        {}
    }
}
