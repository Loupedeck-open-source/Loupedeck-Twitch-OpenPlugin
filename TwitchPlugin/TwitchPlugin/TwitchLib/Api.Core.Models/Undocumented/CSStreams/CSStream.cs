namespace TwitchLib.Api.Core.Models.Undocumented.CSStreams
{
    using System;
    using Newtonsoft.Json;

    public class CSStream
    {
        [JsonProperty(PropertyName = "_id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "game")]
        public String Game { get; protected set; }
        [JsonProperty(PropertyName = "viewers")]
        public Int32 Viewers { get; protected set; }
        [JsonProperty(PropertyName = "map")]
        public String Map { get; protected set; }
        [JsonProperty(PropertyName = "map_name")]
        public String MapName { get; protected set; }
        [JsonProperty(PropertyName = "map_img")]
        public String MapImg { get; protected set; }
        [JsonProperty(PropertyName = "skill")]
        public Int32 Skill { get; protected set; }
        [JsonProperty(PropertyName = "preview")]
        public Preview Preview { get; protected set; }
        [JsonProperty(PropertyName = "is_playlist")]
        public Boolean IsPlaylist { get; protected set; }
        [JsonProperty(PropertyName = "user")]
        public User User { get; protected set; }
    }
}
