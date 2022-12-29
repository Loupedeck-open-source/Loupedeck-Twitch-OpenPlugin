namespace TwitchLib.Client.Models
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>Class used to store credentials used to connect to Twitch chat/whisper.</summary>
    public class ConnectionCredentials
    {
        /// <summary>Property representing URI used to connect to Twitch websocket service.</summary>
        public String TwitchWebsocketURI { get; }
        /// <summary>Property representing bot's oauth.</summary>
        public String TwitchOAuth { get; }
        /// <summary>Property representing bot's username.</summary>
        public String TwitchUsername { get; }
        

        /// <summary>Constructor for ConnectionCredentials object.</summary>
        public ConnectionCredentials(String twitchUsername, String twitchOAuth, String twitchWebsocketURI = "wss://irc-ws.chat.twitch.tv:443", Boolean disableUsernameCheck = false)
        {
            if (!disableUsernameCheck && !new Regex("^([a-zA-Z0-9][a-zA-Z0-9_]{3,25})$").Match(twitchUsername).Success)
                throw new Exception($"Twitch username does not appear to be valid. {twitchUsername}");
            this.TwitchUsername = twitchUsername.ToLower();
            this.TwitchOAuth = twitchOAuth;
            // Make sure proper formatting is applied to oauth
            if (!twitchOAuth.Contains(":"))
            {
                this.TwitchOAuth = $"oauth:{twitchOAuth.Replace("oauth", "")}";
            }
            this.TwitchWebsocketURI = twitchWebsocketURI;
        }
        
    }
}
