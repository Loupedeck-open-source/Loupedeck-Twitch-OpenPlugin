namespace TwitchLib.Api.ThirdParty.ModLookup
{
    using System;
    using Newtonsoft.Json;

    public class TopResponse
    {
        [JsonProperty(PropertyName = "status")]
        public Int32 Status { get; protected set; }
        [JsonProperty(PropertyName = "top")]
        public Top Top { get; protected set; }
    }
}
