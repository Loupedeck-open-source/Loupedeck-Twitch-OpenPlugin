namespace TwitchLib.Api.Helix.Models.StreamsMetadata
{
    using Newtonsoft.Json;

    public class Hearthstone
    {
        [JsonProperty(PropertyName = "broadcaster")]
        public PlayerHearthstone Broadcaster { get; protected set; }
        [JsonProperty(PropertyName = "opponent")]
        public PlayerHearthstone Opponent { get; protected set; }
    }
}
