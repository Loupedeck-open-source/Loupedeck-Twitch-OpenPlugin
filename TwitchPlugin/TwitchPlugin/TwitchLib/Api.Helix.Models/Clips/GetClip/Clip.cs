namespace TwitchLib.Api.Helix.Models.Clips.GetClip
{
    using System;
    using Newtonsoft.Json;

    public class Clip
    {
        [JsonProperty(PropertyName = "id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "url")]
        public String Url { get; protected set; }
        [JsonProperty(PropertyName = "embed_url")]
        public String EmbedUrl { get; protected set; }
        [JsonProperty(PropertyName = "broadcaster_id")]
        public String BroadcasterId { get; protected set; }
        [JsonProperty(PropertyName = "creator_id")]
        public String CreatorId { get; protected set; }
        [JsonProperty(PropertyName = "video_id")]
        public String VideoId { get; protected set; }
        [JsonProperty(PropertyName = "game_id")]
        public String GameId { get; protected set; }
        [JsonProperty(PropertyName = "language")]
        public String Language { get; protected set; }
        [JsonProperty(PropertyName = "title")]
        public String Title { get; protected set; }
        [JsonProperty(PropertyName = "view_count")]
        public Int32 ViewCount { get; protected set; }
        [JsonProperty(PropertyName = "created_at")]
        public String CreatedAt { get; protected set; }
        [JsonProperty(PropertyName = "thumbnail_url")]
        public String ThumbnailUrl { get; protected set; }
    }
}
