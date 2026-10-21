using System;

namespace TPToolkitLib.Exceptions
{
    public class TPException : Exception
    {
        public TPException(string message) : base(message) { }
        public TPException(string message, Exception innerException) : base(message, innerException) { }
    }
}
