namespace TwitchLib.Api.Core.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception representing a provided scope was not permitted.</summary>
    public class BadScopeException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public BadScopeException(String data)
            : base(data)
        {
        }
    }
}