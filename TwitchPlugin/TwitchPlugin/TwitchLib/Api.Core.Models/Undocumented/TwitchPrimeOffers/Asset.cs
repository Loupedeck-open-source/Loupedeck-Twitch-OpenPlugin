namespace TwitchLib.Api.Core.Models.Undocumented.TwitchPrimeOffers
{
    using System;
    using Newtonsoft.Json;

    public class Asset
    {
        [JsonProperty(PropertyName = "assetType")]
        public String AssetType { get; protected set; }
        [JsonProperty(PropertyName = "location")]
        public String Location { get; protected set; }
        [JsonProperty(PropertyName = "location2x")]
        public String Location2x { get; protected set; }
        [JsonProperty(PropertyName = "mediaType")]
        public String MediaType { get; protected set; }
    }
}
