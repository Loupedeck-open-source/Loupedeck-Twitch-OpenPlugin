namespace TwitchLib.Client.Models
{
    using System;
    using Internal;

    public class HostingStarted
    {
        /// <summary>Property representing channel that started hosting.</summary>
        public String HostingChannel;
        /// <summary>Property representing targeted channel, channel being hosted.</summary>
        public String TargetChannel;
        /// <summary>Property representing number of viewers in channel hosting target channel.</summary>
        public Int32 Viewers;

        public HostingStarted(IrcMessage ircMessage)
        {
            var splitted = ircMessage.Message.Split(' ');
            this.HostingChannel = ircMessage.Channel;
            this.TargetChannel = splitted[0];
            this.Viewers = splitted[1].StartsWith("-") ? 0 : Int32.Parse(splitted[1]);
        }

        public HostingStarted(String hostingChannel, String targetChannel, Int32 viewers)
        {
            this.HostingChannel = hostingChannel;
            this.TargetChannel = targetChannel;
            this.Viewers = viewers;
        }
    }
}