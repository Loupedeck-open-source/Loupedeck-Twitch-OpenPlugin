namespace TwitchLib.Api.Helix.Models.Streams
{
    using System;
    using Newtonsoft.Json;

    public class Stream
    {
        [JsonProperty(PropertyName = "id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "user_id")]
        public String UserId { get; protected set; }
        [JsonProperty(PropertyName = "game_id")]
        public String GameId { get; protected set; }
        [JsonProperty(PropertyName = "community_ids")]
        public String[] CommunityIds { get; protected set; }
        [JsonProperty(PropertyName = "type")]
        public String Type { get; protected set; }
        [JsonProperty(PropertyName = "title")]
        public String Title { get; protected set; }
        [JsonProperty(PropertyName = "viewer_count")]
        public Int32 ViewerCount { get; protected set; }
        [JsonProperty(PropertyName = "started_at")]
        public DateTime StartedAt { get; protected set; }
        [JsonProperty(PropertyName = "language")]
        public String Language { get; protected set; }
        [JsonProperty(PropertyName = "thumbnail_url")]
        public String ThumbnailUrl { get; protected set; }
    }
}
