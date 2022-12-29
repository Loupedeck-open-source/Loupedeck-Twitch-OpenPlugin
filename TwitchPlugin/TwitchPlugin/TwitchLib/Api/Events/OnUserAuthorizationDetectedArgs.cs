namespace TwitchLib.Api.Events
{
    using System;
    using System.Collections.Generic;
    using Core.Enums;

    public class OnUserAuthorizationDetectedArgs
    {
        public String Id { get; set; }
        public List<AuthScopes> Scopes { get; set; }
        public String Username { get; set; }
        public String Token { get; set; }
        public String Refresh { get; set; }
    }
}
