namespace TwitchLib.Client.Models
{
    using System;
    using Internal;

    public class HostingStopped
    {
        /// <summary>Property representing hosting channel.</summary>
        public String HostingChannel;
        /// <summary>Property representing number of viewers that were in hosting channel.</summary>
        public Int32 Viewers;

        public HostingStopped(IrcMessage ircMessage)
        {
            var splitted = ircMessage.Message.Split(' ');
            this.HostingChannel = ircMessage.Channel;
            this.Viewers = splitted[1].StartsWith("-") ? 0 : Int32.Parse(splitted[1]);
        }

        public HostingStopped(String hostingChannel, Int32 viewers)
        {
            this.HostingChannel = hostingChannel;
            this.Viewers = viewers;
        }
    }
}