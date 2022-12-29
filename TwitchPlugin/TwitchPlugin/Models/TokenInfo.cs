namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class TokenInfo
    {
        [JsonProperty("client_id")]
        public String ClientId { get; set; }

        [JsonProperty("login")]
        public String Login { get; set; }

        [JsonProperty("scopes")]
        public List<String> Scopes { get; set; }

        [JsonProperty("user_id")]
        public Int32 UserId { get; set; }
    }
}