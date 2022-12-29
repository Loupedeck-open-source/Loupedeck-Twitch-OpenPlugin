namespace TwitchLib.Client.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception thrown when attempting to assign a variable with a different value that is not allowed.</summary>
    public class ClientNotInitializedException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public ClientNotInitializedException(String description)
            : base(description)
        {
        }
    }
}
