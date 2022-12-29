namespace TwitchLib.Api.ThirdParty.ModLookup
{
    using System;
    using Newtonsoft.Json;

    public class ModLookupResponse
    {
        [JsonProperty(PropertyName = "status")]
        public Int32 Status { get; protected set; }
        [JsonProperty(PropertyName = "user")]
        public String User { get; protected set; }
        [JsonProperty(PropertyName = "count")]
        public Int32 Count { get; protected set; }
        [JsonProperty(PropertyName = "channels")]
        public ModLookupListing[] Channels { get; protected set; }
    }
}
