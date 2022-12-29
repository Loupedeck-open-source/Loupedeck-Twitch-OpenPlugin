namespace Loupedeck.TwitchPlugin
{
    using System;
    using Newtonsoft.Json;

    public class Marker
    {
        [JsonProperty(PropertyName = "id")]
        public String Id { get; protected set; }

        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; protected set; }

        [JsonProperty(PropertyName = "position_seconds")]
        public Int32 PositionSeconds { get; protected set; }

        [JsonProperty(PropertyName = "description")]
        public String Description { get; protected set; }
    }
}