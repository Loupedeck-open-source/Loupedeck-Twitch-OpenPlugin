namespace TwitchLib.Client.Models
{
    using System;
    using Internal;

    public class UserBan
    {
        /// <summary>Reason for ban, if it was provided.</summary>
        public String BanReason;
        /// <summary>Channel that had ban event.</summary>
        public String Channel;
        /// <summary>User that was banned.</summary>
        public String Username;


        public UserBan(IrcMessage ircMessage)
        {
            this.Channel = ircMessage.Channel;
            this.Username = ircMessage.Message;

            var successBanReason = ircMessage.Tags.TryGetValue(Tags.BanReason, out var banReason);
            if (successBanReason)
            {
                this.BanReason = banReason;
            }
        }

        public UserBan(String channel, String username, String banReason)
        {
            this.Channel = channel;
            this.Username = username;
            this.BanReason = banReason;
        }
    }
}
