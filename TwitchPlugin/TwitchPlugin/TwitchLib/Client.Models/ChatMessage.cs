namespace TwitchLib.Client.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Enums;
    using Internal;

    /// <summary>Class represents ChatMessage in a Twitch channel.</summary>
    public class ChatMessage : TwitchLibMessage
    {
        protected readonly MessageEmoteCollection _emoteCollection;

        /// <summary>If viewer sent bits in their message, total amount will be here.</summary>
        public Int32 Bits { get; }
        /// <summary>Number of USD (United States Dollars) spent on bits.</summary>
        public Double BitsInDollars { get; }
        /// <summary>Twitch channel message was sent from (useful for multi-channel bots).</summary>
        public String Channel { get; }
        /// <summary>If a cheer badge exists, this property represents the raw value and color (more later). Can be null.</summary>
        public CheerBadge CheerBadge { get; }
        /// <summary>Text after emotes have been handled (if desired). Will be null if replaceEmotes is false.</summary>
        public String EmoteReplacedMessage { get; }
        /// <summary>Unique message identifier assigned by Twitch</summary>
        public String Id { get; }
        /// <summary>Chat message from broadcaster identifier flag</summary>
        public Boolean IsBroadcaster { get; }
        /// <summary>Chat message /me identifier flag.</summary>
        public Boolean IsMe { get; }
        /// <summary>Channel specific moderator status.</summary>
        public Boolean IsModerator { get; }
        /// <summary>Channel specific subscriber status.</summary>
        public Boolean IsSubscriber { get; }
        /// <summary>Twitch chat message contents.</summary>
        public String Message { get; }
        /// <summary>Experimental property noisy determination by Twitch.</summary>
        public Noisy Noisy { get; } = Noisy.NotSet;
        /// <summary>Raw IRC-style text received from Twitch.</summary>
        public String RawIrcMessage { get; }
        /// <summary>Unique identifier of chat room.</summary>
        public String RoomId { get; }
        /// <summary>Number of months a person has been subbed.</summary>
        public Int32 SubscribedMonthCount { get; }        

        //Example IRC message: @badges=moderator/1,warcraft/alliance;color=;display-name=Swiftyspiffyv4;emotes=;mod=1;room-id=40876073;subscriber=0;turbo=0;user-id=103325214;user-type=mod :swiftyspiffyv4!swiftyspiffyv4@swiftyspiffyv4.tmi.twitch.tv PRIVMSG #swiftyspiffy :asd
        /// <summary>Constructor for ChatMessage object.</summary>
        /// <param name="botUsername">The username of the bot that received the message.</param>
        /// <param name="ircMessage">The IRC message from Twitch to be processed.</param>
        /// <param name="emoteCollection">The <see cref="MessageEmoteCollection"/> to register new emotes on and, if desired, use for emote replacement.</param>
        /// <param name="replaceEmotes">Whether to replace emotes for this chat message. Defaults to false.</param>
        public ChatMessage(String botUsername, IrcMessage ircMessage, ref MessageEmoteCollection emoteCollection, Boolean replaceEmotes = false)
        {
            this.BotUsername = botUsername;
            this.RawIrcMessage = ircMessage.ToString();
            this.Message = ircMessage.Message;
            this._emoteCollection = emoteCollection;

            this.Username = ircMessage.User;
            this.Channel = ircMessage.Channel;

            foreach (var tag in ircMessage.Tags.Keys)
            {
                var tagValue = ircMessage.Tags[tag];

                switch (tag)
                {
                    case Tags.Badges:
                        this.Badges = new List<KeyValuePair<String, String>>();
                        var badges = tagValue;
                        if (badges.Contains('/'))
                        {
                            if (!badges.Contains(","))
                                this.Badges.Add(new KeyValuePair<String, String>(badges.Split('/')[0], badges.Split('/')[1]));
                            else
                                foreach (var badge in badges.Split(','))
                                    this.Badges.Add(new KeyValuePair<String, String>(badge.Split('/')[0], badge.Split('/')[1]));
                        }
                        // Iterate through saved badges for special circumstances
                        foreach (var badge in this.Badges)
                        {
                            switch (badge.Key)
                            {
                                case "bits":
                                    this.CheerBadge = new CheerBadge(Int32.Parse(badge.Value));
                                    break;
                                case "subscriber":
                                    this.SubscribedMonthCount = Int32.Parse(badge.Value);
                                    break;
                            }
                        }
                        break;
                    case Tags.Bits:
                        this.Bits = Int32.Parse(tagValue);
                        this.BitsInDollars = ConvertBitsToUsd(this.Bits);
                        break;
                    case Tags.DisplayName:
                        this.DisplayName = tagValue;
                        break;
                    case Tags.Emotes:
                        this.EmoteSet = new EmoteSet(tagValue, this.Message);
                        break;
                    case Tags.Id:
                        this.Id = tagValue;
                        break;
                    case Tags.Mod:
                        this.IsModerator = Common.Helpers.ConvertToBool(tagValue);
                        break;
                    case Tags.Noisy:
                        this.Noisy = Common.Helpers.ConvertToBool(tagValue) ? Noisy.True : Noisy.False;
                        break;
                    case Tags.RoomId:
                        this.RoomId = tagValue;
                        break;
                    case Tags.Subscriber:
                        this.IsSubscriber = Common.Helpers.ConvertToBool(tagValue);
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
                }
            }

            if (this.Message.Length > 0 && (Byte)this.Message[0] == 1 && (Byte)this.Message[this.Message.Length - 1] == 1)
            {
                //Actions (/me {action}) are wrapped by byte=1 and prepended with "ACTION "
                //This setup clears all of that leaving just the action's text.
                //If you want to clear just the nonstandard bytes, use:
                //_message = _message.Substring(1, text.Length-2);
                if (this.Message.StartsWith("\u0001ACTION ") && this.Message.EndsWith("\u0001"))
                {
                    this.Message = this.Message.Trim('\u0001').Substring(7);
                    this.IsMe = true;
                }
            }

            //Parse the emoteSet
            if (this.EmoteSet != null && this.Message != null && this.EmoteSet.Emotes.Count > 0)
            {
                var uniqueEmotes = this.EmoteSet.RawEmoteSetString.Split('/');
                foreach (var emote in uniqueEmotes)
                {
                    var firstColon = emote.IndexOf(':');
                    var firstComma = emote.IndexOf(',');
                    if (firstComma == -1) firstComma = emote.Length;
                    var firstDash = emote.IndexOf('-');
                    if (firstColon > 0 && firstDash > firstColon && firstComma > firstDash)
                    {
                        if (Int32.TryParse(emote.Substring(firstColon + 1, firstDash - firstColon - 1), out var low) &&
                            Int32.TryParse(emote.Substring(firstDash + 1, firstComma - firstDash - 1), out var high))
                        {
                            if (low >= 0 && low < high && high < this.Message.Length)
                            {
                                //Valid emote, let's parse
                                var id = emote.Substring(0, firstColon);
                                //Pull the emote text from the message
                                var text = this.Message.Substring(low, high - low + 1);
                                this._emoteCollection.Add(new MessageEmote(id, text));
                            }
                        }
                    }
                }
                if (replaceEmotes)
                {
                    this.EmoteReplacedMessage = this._emoteCollection.ReplaceEmotes(this.Message);
                }
            }

            if (this.EmoteSet == null)
                this.EmoteSet = new EmoteSet(null, this.Message);

            // Check if display name was set, and if it wasn't, set it to username
            if (String.IsNullOrEmpty(this.DisplayName))
                this.DisplayName = this.Username;

            // Check if message is from broadcaster
            if (String.Equals(this.Channel, this.Username, StringComparison.InvariantCultureIgnoreCase))
            {
                this.UserType = UserType.Broadcaster;
                this.IsBroadcaster = true;
            }

            if (this.Channel.Split(':').Length == 3)
            {
                if (String.Equals(this.Channel.Split(':')[1], this.UserId, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.UserType = UserType.Broadcaster;
                    this.IsBroadcaster = true;
                }
            }
        }

        public ChatMessage(String botUsername, String userId, String userName, String displayName, String colorHex, EmoteSet emoteSet,
            String message, UserType userType, String channel, String id, Boolean isSubscriber, Int32 subscribedMonthCount, String roomId, Boolean isTurbo, Boolean isModerator,
            Boolean isMe, Boolean isBroadcaster, Noisy noisy, String rawIrcMessage, String emoteReplacedMessage, List<KeyValuePair<String, String>> badges,
            CheerBadge cheerBadge, Int32 bits, Double bitsInDollars)
        {
            this.BotUsername = botUsername;
            this.UserId = userId;
            this.DisplayName = displayName;
            this.ColorHex = colorHex;
            this.EmoteSet = emoteSet;
            this.Message = message;
            this.UserType = userType;
            this.Channel = channel;
            this.Id = id;
            this.IsSubscriber = isSubscriber;
            this.SubscribedMonthCount = subscribedMonthCount;
            this.RoomId = roomId;
            this.IsTurbo = isTurbo;
            this.IsModerator = isModerator;
            this.IsMe = isMe;
            this.IsBroadcaster = isBroadcaster;
            this.Noisy = this.Noisy;
            this.RawIrcMessage = rawIrcMessage;
            this.EmoteReplacedMessage = emoteReplacedMessage;
            this.Badges = badges;
            this.CheerBadge = cheerBadge;
            this.Bits = bits;
            this.BitsInDollars = bitsInDollars;
        }

        private static Double ConvertBitsToUsd(Int32 bits)
        {
            /*
            Conversion Rates
            100 bits = $1.40
            500 bits = $7.00
            1500 bits = $19.95 (5%)
            5000 bits = $64.40 (8%)
            10000 bits = $126.00 (10%)
            25000 bits = $308.00 (12%)
            */
            if (bits < 1500)
            {
                return (Double)bits / 100 * 1.4;
            }
            if (bits < 5000)
            {
                return (Double)bits / 1500 * 19.95;
            }
            if (bits < 10000)
            {
                return (Double)bits / 5000 * 64.40;
            }
            if (bits < 25000)
            {
                return (Double)bits / 10000 * 126;
            }
            return (Double)bits / 25000 * 308;
        }
    }
}
