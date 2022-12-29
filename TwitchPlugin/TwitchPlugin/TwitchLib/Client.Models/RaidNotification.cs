namespace TwitchLib.Client.Models
{
    using System;
    using Enums;
    using Internal;

    public class RaidNotification
    {
        public String Badges { get; }
        public String Color { get; }
        public String DisplayName { get; }
        public String Emotes { get; }
        public String Id { get; }
        public String Login { get; }
        public Boolean Moderator { get; }
        public String MsgId { get; }
        public String MsgParamDisplayName { get; }
        public String MsgParamLogin { get; }
        public String MsgParamViewerCount { get; }
        public String RoomId { get; }
        public Boolean Subscriber { get; }
        public String SystemMsg { get; }
        public String SystemMsgParsed { get; }
        public String TmiSentTs { get; }
        public Boolean Turbo { get; }
        public String UserId { get; }
        public UserType UserType { get; }

        // @badges=;color=#FF0000;display-name=Heinki;emotes=;id=4fb7ab2d-aa2c-4886-a286-46e20443f3d6;login=heinki;mod=0;msg-id=raid;msg-param-displayName=Heinki;msg-param-login=heinki;msg-param-viewerCount=4;room-id=27229958;subscriber=0;system-msg=4\sraiders\sfrom\sHeinki\shave\sjoined\n!;tmi-sent-ts=1510249711023;turbo=0;user-id=44110799;user-type= :tmi.twitch.tv USERNOTICE #pandablack
        public RaidNotification(IrcMessage ircMessage)
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
                    case Tags.Login:
                        this.Login = tagValue;
                        break;
                    case Tags.Mod:
                        this.Moderator = Common.Helpers.ConvertToBool(tagValue);
                        break;
                    case Tags.MsgId:
                        this.MsgId = tagValue;
                        break;
                    case Tags.MsgParamDisplayname:
                        this.MsgParamDisplayName = tagValue;
                        break;
                    case Tags.MsgParamLogin:
                        this.MsgParamLogin = tagValue;
                        break;
                    case Tags.MsgParamViewerCount:
                        this.MsgParamViewerCount = tagValue;
                        break;
                    case Tags.RoomId:
                        this.RoomId = tagValue;
                        break;
                    case Tags.Subscriber:
                        this.Subscriber = Common.Helpers.ConvertToBool(tagValue);
                        break;
                    case Tags.SystemMsg:
                        this.SystemMsg = tagValue;
                        this.SystemMsgParsed = tagValue.Replace("\\s", " ").Replace("\\n", "");
                        break;
                    case Tags.TmiSentTs:
                        this.TmiSentTs = tagValue;
                        break;
                    case Tags.Turbo:
                        this.Turbo = Common.Helpers.ConvertToBool(tagValue);
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
        public RaidNotification(String badges, String color, String displayName, String emotes, String id, String login, Boolean moderator, String msgId, String msgParamDisplayName,
            String msgParamLogin, String msgParamViewerCount, String roomId, Boolean subscriber, String systemMsg, String systemMsgParsed, String tmiSentTs, Boolean turbo, UserType userType)
        {
            this.Badges = badges;
            this.Color = color;
            this.DisplayName = displayName;
            this.Emotes = emotes;
            this.Id = id;
            this.Login = login;
            this.Moderator = moderator;
            this.MsgId = msgId;
            this.MsgParamDisplayName = msgParamDisplayName;
            this.MsgParamLogin = msgParamLogin;
            this.MsgParamViewerCount = msgParamViewerCount;
            this.RoomId = roomId;
            this.Subscriber = subscriber;
            this.SystemMsg = systemMsg;
            this.SystemMsgParsed = systemMsgParsed;
            this.TmiSentTs = tmiSentTs;
            this.Turbo = turbo;
            this.UserType = userType;
        }
    }
}

