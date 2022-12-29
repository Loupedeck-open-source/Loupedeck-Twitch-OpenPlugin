namespace TwitchLib.Api.Core.Models.Undocumented.Comments
{
    using System;
    using Newtonsoft.Json;

    public class Comment
    {
        [JsonProperty(PropertyName = "_id")]
        public String Id { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public Object CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public Object UpdatedAt { get; set; }
        [JsonProperty(PropertyName = "channel_id")]
        public String ChannelId { get; set; }
        [JsonProperty(PropertyName = "content_type")]
        public String ContentType { get; set; }
        [JsonProperty(PropertyName = "content_id")]
        public String ContentId { get; set; }
        [JsonProperty(PropertyName = "content_offset_seconds")]
        public Single ContentOffsetSeconds { get; set; }
        [JsonProperty(PropertyName = "commenter")]
        public Commenter Commenter { get; set; }
        [JsonProperty(PropertyName = "source")]
        public String Source { get; set; }
        [JsonProperty(PropertyName = "state")]
        public String State { get; set; }
        [JsonProperty(PropertyName = "message")]
        public Message Message { get; set; }
        [JsonProperty(PropertyName = "more_replies")]
        public Boolean MoreReplies { get; set; }
    }
}