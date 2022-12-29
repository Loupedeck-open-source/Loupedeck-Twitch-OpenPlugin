namespace TwitchLib.Api.Core.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception representing an invalid resource</summary>
    public class BadParameterException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public BadParameterException(String badParamData)
            : base(badParamData)
        {
        }
    }
}
