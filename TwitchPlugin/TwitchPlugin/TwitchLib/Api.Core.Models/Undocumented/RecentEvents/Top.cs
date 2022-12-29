namespace TwitchLib.Api.Core.Models.Undocumented.RecentEvents
{
    using System;
    using Newtonsoft.Json;

    public class Top
    {
        [JsonProperty(PropertyName = "has_top_event")]
        public Boolean HasTopEvent { get; protected set; }
        [JsonProperty(PropertyName = "message_id")]
        public String MessageId { get; protected set; }
        [JsonProperty(PropertyName = "amount")]
        public Int32 Amount { get; protected set; }
        [JsonProperty(PropertyName = "bits_used")]
        public Int32? BitsUsed { get; protected set; }
        [JsonProperty(PropertyName = "message")]
        public String Message { get; protected set; }
        [JsonProperty(PropertyName = "user_id")]
        public String UserId { get; protected set; }
        [JsonProperty(PropertyName = "username")]
        public String Username { get; protected set; }
        //TODO: consider tags param
    }
}
