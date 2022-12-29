namespace TwitchLib.Api.Core.Models.Undocumented.ClipChat
{
    using System;
    using Newtonsoft.Json;

    public class ReChatMessageAttributes
    {
        [JsonProperty(PropertyName = "command")]
        public String Command { get; protected set; }
        [JsonProperty(PropertyName = "room")]
        public String Room { get; protected set; }
        [JsonProperty(PropertyName = "timestamp")]
        public String Timestamp { get; protected set; }
        [JsonProperty(PropertyName = "video-offset")]
        public Int64 VideoOffset { get; protected set; }
        [JsonProperty(PropertyName = "deleted")]
        public Boolean Deleted { get; protected set; }
        [JsonProperty(PropertyName = "message")]
        public String Message { get; protected set; }
        [JsonProperty(PropertyName = "from")]
        public String From { get; protected set; }
        [JsonProperty(PropertyName = "tags")]
        public ReChatMessageAttributesTags Tags { get; protected set; }
        [JsonProperty(PropertyName = "color")]
        public String Color { get; protected set; }
    }
}
