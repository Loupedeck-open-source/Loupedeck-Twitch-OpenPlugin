namespace TwitchLib.Api.Helix.Models.Games
{
    using System;
    using Newtonsoft.Json;

    public class Game
    {
        [JsonProperty(PropertyName = "id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "name")]
        public String Name { get; protected set; }
        [JsonProperty(PropertyName = "box_art_url")]
        public String BoxArtUrl { get; protected set; }
    }
}
