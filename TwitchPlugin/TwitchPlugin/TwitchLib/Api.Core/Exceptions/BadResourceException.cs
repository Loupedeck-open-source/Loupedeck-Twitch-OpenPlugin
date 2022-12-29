namespace TwitchLib.Api.Core.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception representing an invalid resource</summary>
    public class BadResourceException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public BadResourceException(String apiData)
            : base(apiData)
        {
        }
    }
}