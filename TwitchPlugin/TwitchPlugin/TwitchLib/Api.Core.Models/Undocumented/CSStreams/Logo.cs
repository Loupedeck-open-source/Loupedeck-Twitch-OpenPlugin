namespace TwitchLib.Api.Core.Models.Undocumented.CSStreams
{
    using System;
    using Newtonsoft.Json;

    public class Logo
    {
        [JsonProperty(PropertyName = "small")]
        public String Small { get; protected set; }
        [JsonProperty(PropertyName = "medium")]
        public String Medium { get; protected set; }
        [JsonProperty(PropertyName = "large")]
        public String Large { get; protected set; }
        [JsonProperty(PropertyName = "template")]
        public String Template { get; protected set; }
    }
}
