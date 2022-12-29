namespace TwitchLib.Client.Models
{
    using System;
    using Internal;

    public class UserTimeout
    {
        /// <summary>Channel that had timeout event.</summary>
        public String Channel;
        /// <summary>Duration of timeout IN SECONDS.</summary>
        public Int32 TimeoutDuration;
        /// <summary>Reason for timeout, if it was provided.</summary>
        public String TimeoutReason;
        /// <summary>Viewer that was timedout.</summary>
        public String Username;

        public UserTimeout(IrcMessage ircMessage)
        {
            this.Channel = ircMessage.Channel;
            this.Username = ircMessage.Message;

            foreach (var tag in ircMessage.Tags.Keys)
            {
                var tagValue = ircMessage.Tags[tag];

                switch (tag)
                {
                    case Tags.BanDuration:
                        this.TimeoutDuration = Int32.Parse(tagValue);
                        break;
                    case Tags.BanReason:
                        this.TimeoutReason = tagValue;
                        break;
                }
            }
        }

        public UserTimeout(String channel, String username, Int32 timeoutDuration, String timeoutReason)
        {
            this.Channel = channel;
            this.Username = username;
            this.TimeoutDuration = timeoutDuration;
            this.TimeoutReason = timeoutReason;
        }
    }
}
