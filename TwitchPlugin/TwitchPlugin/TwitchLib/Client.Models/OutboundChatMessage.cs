namespace TwitchLib.Client.Models
{
    using System;

    public class OutboundChatMessage
    {        
        public String Channel { get; set; }
        public String Message { get; set; }
        public String Username { get; set; }

        public override String ToString()
        {
            var user = this.Username.ToLower();
            var channel = this.Channel.ToLower();
            return $":{user}!{user}@{user}.tmi.twitch.tv PRIVMSG #{channel} :{this.Message}";
        }
    }
}
