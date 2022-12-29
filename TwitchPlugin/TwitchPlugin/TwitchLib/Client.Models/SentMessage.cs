namespace TwitchLib.Client.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>Model representing a sent message.</summary>
    public class SentMessage
    {
        /// <summary>Badges the sender has</summary>
        public List<KeyValuePair<String, String>> Badges { get; }
        /// <summary>Channel the sent message was sent from.</summary>
        public String Channel { get; }
        /// <summary>Sender's name color.</summary>
        public String ColorHex { get; }
        /// <summary>Display name of the sender.</summary>
        public String DisplayName { get; }
        /// <summary>Emotes that appear in the sent message.</summary>
        public String EmoteSet { get; }
        /// <summary>Whether or not the sender is a moderator.</summary>
        public Boolean IsModerator { get; }
        /// <summary>Whether or not the sender is a subscriber.</summary>
        public Boolean IsSubscriber { get; }
        /// <summary>The message contents.</summary>
        public String Message { get; }
        /// <summary>The type of user (admin, broadcaster, viewer, moderator)</summary>
        public Enums.UserType UserType { get; }


        /// <summary>Model constructor.</summary>
        public SentMessage(UserState state, String message)
        {
            this.Badges = state.Badges;
            this.Channel = state.Channel;
            this.ColorHex = state.ColorHex;
            this.DisplayName = state.DisplayName;
            this.EmoteSet = state.EmoteSet;
            this.IsModerator = state.IsModerator;
            this.IsSubscriber = state.IsSubscriber;
            this.UserType = state.UserType;
            this.Message = message;
        }

        public SentMessage(List<KeyValuePair<String, String>> badges, String channel, String colorHex, String displayName, String emoteSet,
            Boolean isModerator, Boolean isSubscriber, Enums.UserType userType, String message)
        {
            this.Badges = badges;
            this.Channel = channel;
            this.ColorHex = colorHex;
            this.DisplayName = displayName;
            this.EmoteSet = emoteSet;
            this.IsModerator = isModerator;
            this.IsSubscriber = isSubscriber;
            this.UserType = userType;
            this.Message = message;
        }
    }
}
