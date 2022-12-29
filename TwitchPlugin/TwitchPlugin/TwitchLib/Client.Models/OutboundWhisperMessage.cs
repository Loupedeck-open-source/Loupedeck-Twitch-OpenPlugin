namespace TwitchLib.Client.Models
{
    using System;

    public class OutboundWhisperMessage
    {
        public String Username { get; set; }
        public String Receiver { get; set; }
        public String Message { get; set; }

        public override String ToString()
        {
            return $":{this.Username}~{this.Username}@{this.Username}.tmi.twitch.tv PRIVMSG #jtv :/w {this.Receiver} {this.Message}";
        }
    }
}
