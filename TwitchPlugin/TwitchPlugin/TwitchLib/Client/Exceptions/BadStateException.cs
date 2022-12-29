namespace TwitchLib.Client.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception thrown when the state of the client cannot allow an operation to be run.</summary>
    public class BadStateException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public BadStateException(String details)
            : base(details)
        {
        }
    }
}
