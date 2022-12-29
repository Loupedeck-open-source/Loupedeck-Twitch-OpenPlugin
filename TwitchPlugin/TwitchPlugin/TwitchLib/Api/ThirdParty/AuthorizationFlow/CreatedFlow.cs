namespace TwitchLib.Api.ThirdParty.AuthorizationFlow
{
    using System;
    using Newtonsoft.Json;

    public class CreatedFlow
    {
        [JsonProperty(PropertyName = "message")]
        public String Url { get; protected set; }
        [JsonProperty(PropertyName = "id")]
        public String Id { get; protected set; }
    }
}
