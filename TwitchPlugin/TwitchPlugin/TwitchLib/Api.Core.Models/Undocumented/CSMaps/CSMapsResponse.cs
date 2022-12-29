namespace TwitchLib.Api.Core.Models.Undocumented.CSMaps
{
    using System;
    using Newtonsoft.Json;

    public class CSMapsResponse
    {
        [JsonProperty(PropertyName = "_total")]
        public Int32 Total { get; protected set; }
        [JsonProperty(PropertyName = "maps")]
        public Map[] Maps { get; protected set; }
    }
}
