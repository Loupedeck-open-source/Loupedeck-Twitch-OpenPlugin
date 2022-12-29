namespace TwitchLib.Client.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception representing bad information being provided to function/method.</summary>
    public class InvalidParameterException : Exception
    {
        /// <summary>Username that had the exception.</summary>
        public String Username { get; protected set; }

        /// <inheritdoc />
        /// <summary>Exception construtor.</summary>
        public InvalidParameterException(String reasoning, String twitchUsername)
            : base(reasoning)
        {
            this.Username = twitchUsername;
        }
    }
}
