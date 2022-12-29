namespace TwitchLib.Api.Core.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception representing a response received from Twitch that is not expected by the library</summary>
    public class UnexpectedResponseException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public UnexpectedResponseException(String data)
            : base(data)
        {
        }
    }
}
