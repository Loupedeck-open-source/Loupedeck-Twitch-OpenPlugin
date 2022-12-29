namespace TwitchLib.Client.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Enums;
    using Internal;

    /// <summary>Class representing a received whisper from TwitchWhisperClient</summary>
    public class WhisperMessage : TwitchLibMessage
    {
        /// <summary>Property representing message identifier.</summary>
        public String MessageId { get; }
        /// <summary>Property representing identifier of the message thread.</summary>
        public String ThreadId { get; }
        /// <summary>Property representing identifier of the message thread.</summary>
        public String Message { get; }

        public WhisperMessage(List<KeyValuePair<String, String>> badges, String colorHex, String username, String displayName, EmoteSet emoteSet, String threadId, String messageId,
            String userId, Boolean isTurbo, String botUsername, String message, UserType userType)
        {
            this.Badges = badges;
            this.ColorHex = colorHex;
            this.Username = username;
            this.DisplayName = displayName;
            this.EmoteSet = emoteSet;
            this.ThreadId = threadId;
            this.MessageId = messageId;
            this.UserId = userId;
            this.IsTurbo = isTurbo;
            this.BotUsername = botUsername;
            this.Message = message;
            this.UserType = userType;
        }

        /// <summary>
        /// WhisperMessage constructor.
        /// </summary>
        /// <param name="ircMessage">Received IRC string from Twitch server.</param>
        /// <param name="botUsername">Active bot username receiving message.</param>
        public WhisperMessage(IrcMessage ircMessage, String botUsername)
        {
            this.Username = ircMessage.User;
            this.BotUsername = botUsername;

            this.Message = ircMessage.Message;
            foreach (var tag in ircMessage.Tags.Keys)
            {
                var tagValue = ircMessage.Tags[tag];
                switch (tag)
                {
                    case Tags.Badges:
                        this.Badges = new List<KeyValuePair<String, String>>();
                        if (tagValue.Contains('/'))
                        {
                            if (!tagValue.Contains(","))
                                this.Badges.Add(new KeyValuePair<String, String>(tagValue.Split('/')[0], tagValue.Split('/')[1]));
                            else
                                foreach (var badge in tagValue.Split(','))
                                    this.Badges.Add(new KeyValuePair<String, String>(badge.Split('/')[0], badge.Split('/')[1]));
                        }
                        break;
                    case Tags.DisplayName:
                        this.DisplayName = tagValue;
                        break;
                    case Tags.Emotes:
                        this.EmoteSet = new EmoteSet(tagValue, this.Message);
                        break;
                    case Tags.MessageId:
                        this.MessageId = tagValue;
                        break;
                    case Tags.ThreadId:
                        this.ThreadId = tagValue;
                        break;
                    case Tags.Turbo:
                        this.IsTurbo = Common.Helpers.ConvertToBool(tagValue);
                        break;
                    case Tags.UserId:
                        this.UserId = tagValue;
                        break;
                    case Tags.UserType:
                        switch (tagValue)
                        {
                            case "global_mod":
                                this.UserType = UserType.GlobalModerator;
                                break;
                            case "admin":
                                this.UserType = UserType.Admin;
                                break;
                            case "staff":
                                this.UserType = UserType.Staff;
                                break;
                            default:
                                this.UserType = UserType.Viewer;
                                break;
                        }
                        break;
                }
            }

            if (this.EmoteSet == null)
                this.EmoteSet = new EmoteSet(null, this.Message);
        }
    }
}
