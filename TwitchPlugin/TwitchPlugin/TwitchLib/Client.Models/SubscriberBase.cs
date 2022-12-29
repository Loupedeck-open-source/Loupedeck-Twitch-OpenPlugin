namespace TwitchLib.Client.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Enums;
    using Internal;

    /// <summary>Class representing a resubscriber.</summary>
    public class SubscriberBase
    {
        /// <summary>Property representing list of badges assigned.</summary>
        public List<KeyValuePair<String, String>> Badges { get; }
        /// <summary>Property representing the colorhex of the resubscriber.</summary>
        public String ColorHex { get; }
        /// <summary>Property representing HEX color as a System.Drawing.Color object.</summary>
        public String DisplayName { get; }
        /// <summary>Property representing emote set of resubscriber.</summary>
        public String EmoteSet { get; }
        /// <summary>Property representing resub message id</summary>
        public String Id { get; }
        /// <summary>Property representing whether or not the resubscriber is a moderator.</summary>
        public Boolean IsModerator { get; }
        /// <summary>Property representing whether or not person is a partner.</summary>
        public Boolean IsPartner { get; }
        /// <summary>Property representing whether or not the resubscriber is a subscriber (YES).</summary>
        public Boolean IsSubscriber { get; }
        /// <summary>Property representing whether or not the resubscriber is a turbo member.</summary>
        public Boolean IsTurbo { get; }
        /// <summary>Property representing login of resubscription event.</summary>
        public String Login { get; }
        /// <summary>Property representing the raw IRC message (for debugging/customized parsing)</summary>
        public String RawIrc { get; }
        /// <summary>Property representing system message.</summary>
        public String ResubMessage { get; }
        /// <summary>Property representing the room id.</summary>
        public String RoomId { get; }
        /// <summary>Property representing the plan a user is on.</summary>
        public SubscriptionPlan SubscriptionPlan { get; } = SubscriptionPlan.NotSet;
        /// <summary>Property representing the subscription plan name.</summary>
        public String SubscriptionPlanName { get; }
        /// <summary>Property representing internval system message value.</summary>
        public String SystemMessage { get; }
        /// <summary>Property representing internal system message value, parsed.</summary>
        public String SystemMessageParsed { get; }
        /// <summary>Property representing the tmi-sent-ts value.</summary>
        public String TmiSentTs { get; }        
        /// <summary>Property representing the user's id.</summary>
        public String UserId { get; }
        /// <summary>Property representing the user type of the resubscriber.</summary>
        public UserType UserType { get; }
        
        // @badges=subscriber/1,turbo/1;color=#2B119C;display-name=JustFunkIt;emotes=;id=9dasn-asdibas-asdba-as8as;login=justfunkit;mod=0;msg-id=resub;msg-param-months=2;room-id=44338537;subscriber=1;system-msg=JustFunkIt\ssubscribed\sfor\s2\smonths\sin\sa\srow!;turbo=1;user-id=26526370;user-type= :tmi.twitch.tv USERNOTICE #burkeblack :AVAST YEE SCURVY DOG

        protected readonly Int32 months;

        /// <summary>Subscriber object constructor.</summary>
        protected SubscriberBase(IrcMessage ircMessage)
        {
            this.RawIrc = ircMessage.ToString();
            this.ResubMessage = ircMessage.Message;

            foreach (var tag in ircMessage.Tags.Keys)
            {
                var tagValue = ircMessage.Tags[tag];
                switch (tag)
                {
                    case Tags.Badges:
                        this.Badges = new List<KeyValuePair<String, String>>();
                        foreach (var badgeValue in tagValue.Split(','))
                            if (badgeValue.Contains('/'))
                                this.Badges.Add(new KeyValuePair<String, String>(badgeValue.Split('/')[0], badgeValue.Split('/')[1]));
                        // iterate through badges for special circumstances
                        foreach (var badge in this.Badges)
                        {
                            if (badge.Key == "partner")
                                this.IsPartner = true;
                        }
                        break;
                    case Tags.DisplayName:
                        this.DisplayName = tagValue;
                        break;
                    case Tags.Emotes:
                        this.EmoteSet = tagValue;
                        break;
                    case Tags.Id:
                        this.Id = tagValue;
                        break;
                    case Tags.Login:
                        this.Login = tagValue;
                        break;
                    case Tags.Mod:
                        this.IsModerator = ConvertToBool(tagValue);
                        break;
                    case Tags.MsgParamMonths:
                        this.months = Int32.Parse(tagValue);
                        break;
                    case Tags.MsgParamSubPlan:
                        switch (tagValue.ToLower())
                        {
                            case "prime":
                                this.SubscriptionPlan = SubscriptionPlan.Prime;
                                break;
                            case "1000":
                                this.SubscriptionPlan = SubscriptionPlan.Tier1;
                                break;
                            case "2000":
                                this.SubscriptionPlan = SubscriptionPlan.Tier2;
                                break;
                            case "3000":
                                this.SubscriptionPlan = SubscriptionPlan.Tier3;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(tagValue.ToLower));
                        }
                        break;
                    case Tags.MsgParamSubPlanName:
                        this.SubscriptionPlanName = tagValue.Replace("\\s", " ");
                        break;
                    case Tags.RoomId:
                        this.RoomId = tagValue;
                        break;
                    case Tags.Subscriber:
                        this.IsSubscriber = ConvertToBool(tagValue);
                        break;
                    case Tags.SystemMsg:
                        this.SystemMessage = tagValue;
                        this.SystemMessageParsed = tagValue.Replace("\\s", " ");
                        break;
                    case Tags.TmiSentTs:
                        this.TmiSentTs = tagValue;
                        break;
                    case Tags.Turbo:
                        this.IsTurbo = ConvertToBool(tagValue);
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
        }

        internal SubscriberBase(List<KeyValuePair<String, String>> badges, String colorHex, String displayName, String emoteSet, String id, String login, String systemMessage,
            String systemMessageParsed, String resubMessage, SubscriptionPlan subscriptionPlan, String subscriptionPlanName, String roomId, String userId, Boolean isModerator, Boolean isTurbo,
            Boolean isSubscriber, Boolean isPartner, String tmiSentTs, UserType userType, String rawIrc, String channel)
        {
            this.Badges = badges;
            this.ColorHex = colorHex;
            this.DisplayName = displayName;
            this.EmoteSet = emoteSet;
            this.Id = id;
            this.Login = login;
            this.SystemMessage = systemMessage;
            this.SystemMessageParsed = systemMessageParsed;
            this.ResubMessage = resubMessage;
            this.SubscriptionPlan = subscriptionPlan;
            this.SubscriptionPlanName = subscriptionPlanName;
            this.RoomId = roomId;
            this.UserId = this.UserId;
            this.IsModerator = isModerator;
            this.IsTurbo = isTurbo;
            this.IsSubscriber = isSubscriber;
            this.IsPartner = isPartner;
            this.TmiSentTs = tmiSentTs;
            this.UserType = userType;
            this.RawIrc = rawIrc;
        }

        private static Boolean ConvertToBool(String data)
        {
            return data == "1";
        }

        /// <summary>Overriden ToString method, prints out all properties related to resub.</summary>
        public override String ToString()
        {
            return $"Badges: {this.Badges.Count}, color hex: {this.ColorHex}, display name: {this.DisplayName}, emote set: {this.EmoteSet}, login: {this.Login}, system message: {this.SystemMessage}, " +
                $"resub message: {this.ResubMessage}, months: {this.months}, room id: {this.RoomId}, user id: {this.UserId}, mod: {this.IsModerator}, turbo: {this.IsTurbo}, sub: {this.IsSubscriber}, user type: {this.UserType}, raw irc: {this.RawIrc}";
        }
    }
}
