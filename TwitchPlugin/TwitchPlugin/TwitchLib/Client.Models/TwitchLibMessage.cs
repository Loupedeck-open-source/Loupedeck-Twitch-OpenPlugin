namespace TwitchLib.Client.Models
{
    using System;
    using System.Collections.Generic;
    using Enums;

    /// <summary>Class represents Message.</summary>
    public abstract class TwitchLibMessage
    {
        /// <summary>List of key-value pair badges.</summary>
        public List<KeyValuePair<String, String>> Badges { get; protected set;}
        /// <summary>Twitch username of the bot that received the message.</summary>
        public String BotUsername { get; protected set;}
        /// <summary>Hex representation of username color in chat (THIS CAN BE NULL IF VIEWER HASN'T SET COLOR).</summary>
        public String ColorHex { get; protected set;}
        /// <summary>Case-sensitive username of sender of chat message.</summary>
        public String DisplayName { get; protected set;}
        /// <summary>Emote Ids that exist in message.</summary>
        public EmoteSet EmoteSet { get; protected set;}
        /// <summary>Twitch site-wide turbo status.</summary>
        public Boolean IsTurbo { get; protected set;}
        /// <summary>Twitch-unique integer assigned on per account basis.</summary>
        public String UserId { get; protected set;}
        /// <summary>Username of sender of chat message.</summary>
        public String Username { get; protected set;}
        /// <summary>User type can be viewer, moderator, global mod, admin, or staff</summary>
        public UserType UserType { get; protected set;}
    }
}
