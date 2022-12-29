namespace Loupedeck.TwitchPlugin
{
    using System;

    public class UserInfo
    {
        public UserInfo(Int32 id, String login)
        {
            this.Id = id;
            this.Login = login;
        }

        public Int32 Id { get; set; }
        public String Login { get; set; }
    }
}