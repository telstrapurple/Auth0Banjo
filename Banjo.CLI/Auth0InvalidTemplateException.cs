#nullable enable
using System;
using System.Runtime.Serialization;

namespace Banjo.CLI
{
    public class Auth0InvalidTemplateException : Exception
    {
        public Auth0InvalidTemplateException()
        {
        }

        protected Auth0InvalidTemplateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public Auth0InvalidTemplateException(string? message) : base(message)
        {
        }

        public Auth0InvalidTemplateException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}