namespace TwitchLib.Api.Core.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception representing a 502 Http Statuscode</summary>
    public class BadGatewayException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public BadGatewayException(String data)
            : base(data)
        {
        }
    }
}