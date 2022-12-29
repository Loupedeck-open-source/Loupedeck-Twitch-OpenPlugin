namespace TwitchLib.Client.Models
{
    using System;
    using Enums;
    using Internal;

    public class CommunitySubscription
    {
        public String Badges;
        public String Color;
        public String DisplayName;
        public String Emotes;
        public String Id;
        public String Login;
        public Boolean IsModerator;
        public String MsgId;
        public Int32 MsgParamMassGiftCount;
        public Int32 MsgParamSenderCount;
        public SubscriptionPlan MsgParamSubPlan;
        public String RoomId;
        public Boolean IsSubscriber;
        public String SystemMsg;
        public String SystemMsgParsed;
        public String TmiSentTs;
        public Boolean IsTurbo;
        public String UserId;
        public UserType UserType;

        public CommunitySubscription(IrcMessage ircMessage)
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
                    case Tags.MsgParamMassGiftCount:
                        this.MsgParamMassGiftCount = Int32.Parse(tagValue);
                        break;
                    case Tags.MsgParamSenderCount:
                        this.MsgParamSenderCount = Int32.Parse(tagValue);
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
    }
}
