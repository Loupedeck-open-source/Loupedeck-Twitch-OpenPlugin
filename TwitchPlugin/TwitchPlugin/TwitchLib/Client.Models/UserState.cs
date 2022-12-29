namespace TwitchLib.Client.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Enums;
    using Internal;

    /// <summary>Class representing state of a specific user.</summary>
    public class UserState
    {
        /// <summary>Properrty representing the chat badges a specific user has.</summary>
        public List<KeyValuePair<String, String>> Badges { get; } = new List<KeyValuePair<String, String>>();
        /// <summary>Property representing channel.</summary>
        public String Channel { get; }
        /// <summary>Properrty representing HEX user's name.</summary>
        public String ColorHex { get; }
        /// <summary>Property representing user's display name.</summary>
        public String DisplayName { get; }
        /// <summary>Property representing emote sets available to user.</summary>
        public String EmoteSet { get; }
        /// <summary>Property representing Turbo status.</summary>
        public Boolean IsModerator { get; }
        /// <summary>Property representing subscriber status.</summary>
        public Boolean IsSubscriber { get; }        
        /// <summary>Property representing returned user type of user.</summary>
        public UserType UserType { get; }
    
        /// <summary>
        /// Constructor for UserState.
        /// </summary>
        /// <param name="ircMessage"></param>
        public UserState(IrcMessage ircMessage)
        {
            this.Channel = ircMessage.Channel;

            foreach (var tag in ircMessage.Tags.Keys)
            {
                var tagValue = ircMessage.Tags[tag];
                switch (tag)
                {
                    case Tags.Badges:
                        if (tagValue.Contains('/'))
                        {
                            if (!tagValue.Contains(","))
                                this.Badges.Add(new KeyValuePair<String, String>(tagValue.Split('/')[0], tagValue.Split('/')[1]));
                            else
                                foreach (var badge in tagValue.Split(','))
                                    this.Badges.Add(new KeyValuePair<String, String>(badge.Split('/')[0], badge.Split('/')[1]));
                        }
                        break;
                    case Tags.Color:
                        this.ColorHex = tagValue;
                        break;
                    case Tags.DisplayName:
                        this.DisplayName = tagValue;
                        break;
                    case Tags.EmotesSets:
                        this.EmoteSet = tagValue;
                        break;
                    case Tags.Mod:
                        this.IsModerator = Common.Helpers.ConvertToBool(tagValue);
                        break;
                    case Tags.Subscriber:
                        this.IsSubscriber = Common.Helpers.ConvertToBool(tagValue);
                        break;
                    case Tags.UserType:
                        switch (tagValue)
                        {
                            case "mod":
                                this.UserType = UserType.Moderator;
                                break;
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
                    default:
                        // This should never happen, unless Twitch changes their shit
                        Console.WriteLine($"Unaccounted for [UserState]: {tag}");
                        break;
                }
            }

            if (String.Equals(ircMessage.User, this.Channel, StringComparison.InvariantCultureIgnoreCase))
                this.UserType = UserType.Broadcaster;
        }

        public UserState(List<KeyValuePair<String, String>> badges, String colorHex, String displayName, String emoteSet, String channel,
            Boolean isSubscriber, Boolean isModerator, UserType userType)
        {
            this.Badges = badges;
            this.ColorHex = colorHex;
            this.DisplayName = displayName;
            this.EmoteSet = emoteSet;
            this.Channel = channel;
            this.IsSubscriber = isSubscriber;
            this.IsModerator = isModerator;
            this.UserType = this.UserType;
        }
    }
}
