namespace Loupedeck.TwitchPlugin
{
    using System;

    public class UserInfo
    {
        public UserInfo(String id, String login)
        {
            this.Id = id;
            this.Login = login;
        }

        public String Id { get; set; }
        public String Login { get; set; }
    }
}