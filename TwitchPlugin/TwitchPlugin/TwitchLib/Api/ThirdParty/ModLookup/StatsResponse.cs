namespace TwitchLib.Api.ThirdParty.ModLookup
{
    using System;
    using Newtonsoft.Json;

    public class StatsResponse
    {
        [JsonProperty(PropertyName = "status")]
        public Int32 Status { get; protected set; }
        [JsonProperty(PropertyName = "stats")]
        public Stats Stats { get; protected set; }
    }
}
