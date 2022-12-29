namespace TwitchLib.Api.ThirdParty.ModLookup
{
    using System;
    using Newtonsoft.Json;

    public class ModLookupListing
    {
        [JsonProperty(PropertyName = "name")]
        public String Name { get; protected set; }
        [JsonProperty(PropertyName = "followers")]
        public Int32 Followers { get; protected set; }
        [JsonProperty(PropertyName = "views")]
        public Int32 Views { get; protected set; }
        [JsonProperty(PropertyName = "partnered")]
        public Boolean Partnered { get; protected set; }
    }
}
