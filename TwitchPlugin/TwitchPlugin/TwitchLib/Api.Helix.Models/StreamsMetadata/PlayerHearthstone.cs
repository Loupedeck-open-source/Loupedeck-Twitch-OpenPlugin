namespace TwitchLib.Api.Helix.Models.StreamsMetadata
{
    using Newtonsoft.Json;

    public class PlayerHearthstone
    {
        [JsonProperty(PropertyName = "hero")]
        public HeroHearthstone Hero { get; protected set; }
    }
}
