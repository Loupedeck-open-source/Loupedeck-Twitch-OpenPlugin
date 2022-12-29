namespace TwitchLib.Api.Helix.Models.Videos
{
    using System;
    using Newtonsoft.Json;

    public class Video
    {
        [JsonProperty(PropertyName = "created_at")]
        public String CreatedAt { get; protected set; }
        [JsonProperty(PropertyName = "description")]
        public String Description { get; protected set; }
        [JsonProperty(PropertyName = "duration")]
        public String Duration { get; protected set; }
        [JsonProperty(PropertyName = "id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "language")]
        public String Language { get; protected set; }
        [JsonProperty(PropertyName = "published_at")]
        public String PublishedAt { get; protected set; }
        [JsonProperty(PropertyName = "thumbnail_url")]
        public String ThumbnailUrl { get; protected set; }
        [JsonProperty(PropertyName = "title")]
        public String Title { get; protected set; }
        [JsonProperty(PropertyName = "user_id")]
        public String UserId { get; protected set; }
        [JsonProperty(PropertyName = "view_count")]
        public Int32 ViewCount { get; protected set; }
    }
}
