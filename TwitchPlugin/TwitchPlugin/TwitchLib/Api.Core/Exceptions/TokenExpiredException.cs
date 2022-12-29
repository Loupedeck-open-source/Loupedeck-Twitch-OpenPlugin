namespace TwitchLib.Api.Core.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception representing a detection that the OAuth token expired</summary>
    public class TokenExpiredException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public TokenExpiredException(String data)
            : base(data)
        {
        }
    }
}