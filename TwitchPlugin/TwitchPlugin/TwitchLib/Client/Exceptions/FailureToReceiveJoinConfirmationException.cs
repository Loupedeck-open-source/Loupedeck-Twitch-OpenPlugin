namespace TwitchLib.Client.Exceptions
{
    using System;

    public class FailureToReceiveJoinConfirmationException
    {
        /// <summary>Exception representing failure of client to receive JOIN confirmation.</summary>
        public String Channel { get; protected set; }
        /// <summary>Extra details regarding this exception (not always set)</summary>
        public String Details { get; protected set; }

        /// <summary>Exception construtor.</summary>
        public FailureToReceiveJoinConfirmationException(String channel, String details = null)
        {
            this.Channel = channel;
            this.Details = details;
        }
    }
}
