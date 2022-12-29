namespace TwitchLib.Client.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception thrown when attempting to perform an actino that is only available when the client is connected.</summary>
    public class ClientNotConnectedException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public ClientNotConnectedException(String description)
            : base(description)
        {
        }
    }
}
