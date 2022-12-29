namespace TwitchLib.Api.ThirdParty.UsernameChange
{
    using System;
    using Newtonsoft.Json;

    public class UsernameChangeListing
    {
        [JsonProperty(PropertyName = "userid")]
        public String UserId { get; protected set; }
        [JsonProperty(PropertyName = "username_old")]
        public String UsernameOld { get; protected set; }
        [JsonProperty(PropertyName = "username_new")]
        public String UsernameNew { get; protected set; }
        [JsonProperty(PropertyName = "found_at")]
        public DateTime FoundAt { get; protected set; }
    }
}
