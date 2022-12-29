namespace TwitchLib.Api.Core.Models.Undocumented.RecentEvents
{
    using System;
    using Newtonsoft.Json;

    public class Recent
    {
        [JsonProperty(PropertyName = "has_recent_events")]
        public Boolean HasRecentEvents { get; protected set; }
        [JsonProperty(PropertyName = "message_id")]
        public String MessageId { get; protected set; }
        [JsonProperty(PropertyName = "timestamp")]
        public String Timestamp { get; protected set; }
        [JsonProperty(PropertyName = "channel_id")]
        public String ChannelId { get; protected set; }
        [JsonProperty(PropertyName = "allotted_time_ms")]
        public Int64 AllottedTimeMs { get; protected set; }
        [JsonProperty(PropertyName = "time_remaining_ms")]
        public Int64 TimeRemainingMs { get; protected set; }
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
        //TODO: consider tags property
    }
}
