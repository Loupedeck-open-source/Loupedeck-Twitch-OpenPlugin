namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Token
    {
        [JsonProperty("access_token")]
        public String AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public String RefreshToken { get; set; }

        [JsonProperty("scope")]
        public List<String> Scopes { get; set; }
    }
}