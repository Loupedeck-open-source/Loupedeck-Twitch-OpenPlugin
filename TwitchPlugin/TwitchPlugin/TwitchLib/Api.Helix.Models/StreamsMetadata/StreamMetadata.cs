namespace TwitchLib.Api.Helix.Models.StreamsMetadata
{
    using System;
    using Newtonsoft.Json;

    public class StreamMetadata
    {
        [JsonProperty(PropertyName = "user_id")]
        public String UserId { get; protected set; }
        [JsonProperty(PropertyName = "game_id")]
        public String GameId { get; protected set; }
        [JsonProperty(PropertyName = "hearthstone")]
        public Hearthstone Hearthstone { get; protected set; }
        [JsonProperty(PropertyName = "overwatch")]
        public Overwatch Overwatch { get; protected set; }
    }
}
