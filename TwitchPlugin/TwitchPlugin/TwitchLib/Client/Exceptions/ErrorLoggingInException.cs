namespace TwitchLib.Client.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception representing credentials provided for logging in were bad.</summary>
    public class ErrorLoggingInException : Exception
    {
        /// <summary>Exception representing username associated with bad login.</summary>
        public String Username { get; protected set; }

        /// <inheritdoc />
        /// <summary>Exception construtor.</summary>
        public ErrorLoggingInException(String ircData, String twitchUsername)
            : base(ircData)
        {
            this.Username = twitchUsername;
        }
    }
}