namespace TwitchLib.Api.Core.Models.Undocumented.CSMaps
{
    using System;
    using Newtonsoft.Json;

    public class Map
    {
        [JsonProperty(PropertyName = "map")]
        public String MapCode { get; protected set; }
        [JsonProperty(PropertyName = "map_name")]
        public String MapName { get; protected set; }
        [JsonProperty(PropertyName = "map_image")]
        public String MapImage { get; protected set; }
        [JsonProperty(PropertyName = "viewers")]
        public Int32 Viewers { get; protected set; }
    }
}
