﻿namespace TwitchLib.Api.ThirdParty.ModLookup
{
    using Newtonsoft.Json;

    public class Top
    {
        [JsonProperty(PropertyName = "modcount")]
        public ModLookupListing[] ModCount { get; protected set; }
        [JsonProperty(PropertyName = "views")]
        public ModLookupListing[] Views { get; protected set; }
        [JsonProperty(PropertyName = "followers")]
        public ModLookupListing[] Followers { get; protected set; }
    }
}
