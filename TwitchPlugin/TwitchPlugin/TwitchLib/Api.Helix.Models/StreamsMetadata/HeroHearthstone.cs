namespace TwitchLib.Api.Helix.Models.StreamsMetadata
{
    using System;
    using Newtonsoft.Json;

    public class HeroHearthstone
    {
        [JsonProperty(PropertyName = "class")]
        public String Class { get; protected set; }
        [JsonProperty(PropertyName = "name")]
        public String Name { get; protected set; }
        [JsonProperty(PropertyName = "type")]
        public String Type { get; protected set; }
    }
}
