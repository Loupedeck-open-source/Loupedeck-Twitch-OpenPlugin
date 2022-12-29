namespace TwitchLib.Client.Models
{
    using System;
    using Enums;
    using Internal;

    public class RitualNewChatter
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
        public String Message { get; }
        public String MsgId { get; }
        public String MsgParamRitualName { get; }
        public String RoomId { get; }        
        public String SystemMsgParsed { get; }
        public String SystemMsg { get; }
        public String TmiSentTs { get; }        
        public String UserId { get; }
        public UserType UserType { get; }
        

        // badges=subscriber/0;color=#0000FF;display-name=KittyJinxu;emotes=30259:0-6;id=1154b7c0-8923-464e-a66b-3ef55b1d4e50;
        // login=kittyjinxu;mod=0;msg-id=ritual;msg-param-ritual-name=new_chatter;room-id=35740817;subscriber=1;
        // system-msg=@KittyJinxu\sis\snew\shere.\sSay\shello!;tmi-sent-ts=1514387871555;turbo=0;user-id=187446639;
        // user-type= USERNOTICE #thorlar kittyjinxu > #thorlar: HeyGuys
        public RitualNewChatter(IrcMessage ircMessage)
        {
            this.Message = ircMessage.Message;
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
                    case Tags.MsgParamRitualName:
                        this.MsgParamRitualName = tagValue;
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
    }
}
