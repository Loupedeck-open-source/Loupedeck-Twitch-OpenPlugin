namespace TwitchLib.Client.Models
{
    using System;
    using Enums;
    using Internal;

    public class GiftedSubscription
    {
        public String Badges { get; }
        public String Color { get; }
        public String DisplayName { get; }
        public String Emotes { get; }
        public String Id { get; }
        public Boolean IsModerator { get; }
        public Boolean IsSubscriber { get; }
        public Boolean IsTurbo { get; }
        public String Login { get; }        
        public String MsgId { get; }
        public String MsgParamMonths { get; }
        public String MsgParamRecipientDisplayName { get; }
        public String MsgParamRecipientId { get; }
        public String MsgParamRecipientUserName { get; }
        public String MsgParamSubPlanName { get; }
        public SubscriptionPlan MsgParamSubPlan { get; }
        public String RoomId { get; }        
        public String SystemMsg { get; }
        public String SystemMsgParsed { get; }
        public String TmiSentTs { get; }        
        public UserType UserType { get; }

        public GiftedSubscription(IrcMessage ircMessage)
        {
            foreach (var tag in ircMessage.Tags.Keys)
            {
                var tagValue = ircMessage.Tags[tag];

                switch (tag)
                {
                    case Tags.Badges:
                        this.Badges = tagValue;
                        break;
                    case Tags.Color:
                        this.Color = tagValue;
                        break;
                    case Tags.DisplayName:
                        this.DisplayName = tagValue;
                        break;
                    case Tags.Emotes:
                        this.Emotes = tagValue;
                        break;
                    case Tags.Id:
                        this.Id = tagValue;
                        break;
                    case Tags.Login:
                        this.Login = tagValue;
                        break;
                    case Tags.Mod:
                        this.IsModerator = Common.Helpers.ConvertToBool(tagValue);
                        break;
                    case Tags.MsgId:
                        this.MsgId = tagValue;
                        break;
                    case Tags.MsgParamMonths:
                        this.MsgParamMonths = tagValue;
                        break;
                    case Tags.MsgParamRecipientDisplayname:
                        this.MsgParamRecipientDisplayName = tagValue;
                        break;
                    case Tags.MsgParamRecipientId:
                        this.MsgParamRecipientId = tagValue;
                        break;
                    case Tags.MsgParamRecipientUsername:
                        this.MsgParamRecipientUserName = tagValue;
                        break;
                    case Tags.MsgParamSubPlanName:
                        this.MsgParamSubPlanName = tagValue;
                        break;
                    case Tags.MsgParamSubPlan:
                        switch (tagValue)
                        {
                            case "prime":
                                this.MsgParamSubPlan = SubscriptionPlan.Prime;
                                break;
                            case "1000":
                                this.MsgParamSubPlan = SubscriptionPlan.Tier1;
                                break;
                            case "2000":
                                this.MsgParamSubPlan = SubscriptionPlan.Tier2;
                                break;
                            case "3000":
                                this.MsgParamSubPlan = SubscriptionPlan.Tier3;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(tagValue.ToLower));
                        }
                        break;
                    case Tags.RoomId:
                        this.RoomId = tagValue;
                        break;
                    case Tags.Subscriber:
                        this.IsSubscriber = Common.Helpers.ConvertToBool(tagValue);
                        break;
                    case Tags.SystemMsg:
                        this.SystemMsg = tagValue;
                        this.SystemMsgParsed = tagValue.Replace("\\s", " ").Replace("\\n", "");
                        break;
                    case Tags.TmiSentTs:
                        this.TmiSentTs = tagValue;
                        break;
                    case Tags.Turbo:
                        this.IsTurbo = Common.Helpers.ConvertToBool(tagValue);
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
        public GiftedSubscription(String badges, String color, String displayName, String emotes, String id, String login, Boolean isModerator,
            String msgId, String msgParamMonths, String msgParamRecipientDisplayName, String msgParamRecipientId, String msgParamRecipientUserName,
            String msgParamSubPlanName, SubscriptionPlan msgParamSubPlan, String roomId, Boolean isSubscriber, String systemMsg, String systemMsgParsed,
            String tmiSentTs, Boolean isTurbo, UserType userType)
        {
            this.Badges = badges;
            this.Color = color;
            this.DisplayName = displayName;
            this.Emotes = emotes;
            this.Id = id;
            this.Login = login;
            this.IsModerator = isModerator;
            this.MsgId = msgId;
            this.MsgParamMonths = msgParamMonths;
            this.MsgParamRecipientDisplayName = msgParamRecipientDisplayName;
            this.MsgParamRecipientId = msgParamRecipientId;
            this.MsgParamRecipientUserName = msgParamRecipientUserName;
            this.MsgParamSubPlanName = msgParamSubPlanName;
            this.MsgParamSubPlan = msgParamSubPlan;
            this.RoomId = roomId;
            this.IsSubscriber = isSubscriber;
            this.SystemMsg = systemMsg;
            this.SystemMsgParsed = systemMsgParsed;
            this.TmiSentTs = tmiSentTs;
            this.IsTurbo = isTurbo;
            this.UserType = userType;
        }
    }
}
