namespace TwitchLib.Api.ThirdParty.AuthorizationFlow
{
    using System;
    using Newtonsoft.Json;

    public class RefreshTokenResponse
    {
        [JsonProperty(PropertyName = "token")]
        public String Token { get; protected set; }
        [JsonProperty(PropertyName = "refresh")]
        public String Refresh { get; protected set; }
    }
}
