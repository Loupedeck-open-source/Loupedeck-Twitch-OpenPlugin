namespace TwitchLib.Api.Helix.Models.StreamsMetadata
{
    using System;
    using Newtonsoft.Json;

    public class HeroOverwatch
    {
        [JsonProperty(PropertyName = "ability")]
        public String Ability { get; protected set; }
        [JsonProperty(PropertyName = "name")]
        public String Name { get; protected set; }
        [JsonProperty(PropertyName = "role")]
        public String Role { get; protected set; }
    }
}
