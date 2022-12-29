namespace TwitchLib.Api.Helix.Models.StreamsMetadata
{
    using Newtonsoft.Json;

    public class PlayerOverwatch
    {
        [JsonProperty(PropertyName = "hero")]
        public HeroOverwatch Hero { get; protected set; }
    }
}
