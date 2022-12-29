
namespace TwitchLib.Api.Core.Models.Undocumented.Chatters
{
    using System;

    public class ChatterFormatted
    {
        public String Username { get; protected set; }
        public Enums.UserType UserType { get;  set; }

        public ChatterFormatted(String username, Enums.UserType userType)
        {
            this.Username = username;
            this.UserType = userType;
        }

        
    }
}
