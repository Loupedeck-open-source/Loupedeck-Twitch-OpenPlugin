namespace TwitchLib.Client.Models
{
    using System;
    using System.Linq;
    using Internal;

    public partial class BeingHostedNotification
    {
        public String BotUsername { get; private set; }
        public String Channel { get; }        
        public String HostedByChannel { get; }
        public Boolean IsAutoHosted { get; }
        public Int32 Viewers { get; }
        

        public BeingHostedNotification(String botUsername, IrcMessage ircMessage)
        {
            this.Channel = ircMessage.Channel;
            this.BotUsername = botUsername;
            this.HostedByChannel = ircMessage.Message.Split(' ').First();

            if (ircMessage.Message.Contains("up to "))
            {
                var splt = ircMessage.Message.Split(new String[] { "up to " }, StringSplitOptions.None);
                if (splt[1].Contains(" ") && Int32.TryParse(splt[1].Split(' ')[0], out Int32 n))
                    this.Viewers = n;
            }
                
            if (ircMessage.Message.Contains("auto hosting"))
                this.IsAutoHosted = true;
        }

        public BeingHostedNotification(String channel, String botUsername, String hostedByChannel, Int32 viewers, Boolean isAutoHosted)
        {
            this.Channel = channel;
            this.BotUsername = botUsername;
            this.HostedByChannel = hostedByChannel;
            this.Viewers = viewers;
            this.IsAutoHosted = isAutoHosted;
        }
    }
}
