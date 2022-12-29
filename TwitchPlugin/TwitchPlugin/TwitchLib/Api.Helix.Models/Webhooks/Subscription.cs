namespace TwitchLib.Api.Helix.Models.Webhooks
{
    using System;
    using Newtonsoft.Json;

    public class Subscription
    {
        [JsonProperty(PropertyName = "topic")]
        public String Topic { get; protected set; }
        [JsonProperty(PropertyName = "callback")]
        public String Callback { get; protected set; }
        [JsonProperty(PropertyName = "expires_at")]
        public DateTime ExpiresAt { get; protected set; }
    }
}
