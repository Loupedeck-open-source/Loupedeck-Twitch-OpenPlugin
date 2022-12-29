namespace TwitchLib.Api.Core.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception representing a 504 Http Statuscode</summary>
    public class GatewayTimeoutException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public GatewayTimeoutException(String data)
            : base(data)
        {
        }
    }
}