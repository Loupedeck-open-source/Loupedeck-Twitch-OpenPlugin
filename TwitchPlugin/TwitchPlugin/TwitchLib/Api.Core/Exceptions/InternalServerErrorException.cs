namespace TwitchLib.Api.Core.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception representing a 500 Http Statuscode</summary>
    public class InternalServerErrorException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public InternalServerErrorException(String data)
            : base(data)
        {
        }
    }
}