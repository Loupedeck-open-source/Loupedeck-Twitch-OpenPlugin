namespace TwitchLib.Api.Helix.Models.Bits
{
    using System;
    using Newtonsoft.Json;

    public class Listing
    {
        [JsonProperty(PropertyName = "user_id")]
        public String UserId { get; protected set; }
        [JsonProperty(PropertyName = "rank")]
        public Int32 Rank { get; protected set; }
        [JsonProperty(PropertyName = "score")]
        public Int32 Score { get; protected set; }
    }
}
