#nullable enable
using System;
using System.Runtime.Serialization;

namespace Banjo.CLI
{
    public class Auth0ResourceNotFoundException : Exception
    {
        public Auth0ResourceNotFoundException()
        {
        }

        protected Auth0ResourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public Auth0ResourceNotFoundException(string? message) : base(message)
        {
        }

        public Auth0ResourceNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}