namespace TwitchLib.Api.Core.Models.Undocumented.ChannelPanels
{
    using System;
    using Newtonsoft.Json;

    public class Data
    {
        [JsonProperty(PropertyName = "link")]
        public String Link { get; protected set; }
        [JsonProperty(PropertyName = "image")]
        public String Image { get; protected set; }
        [JsonProperty(PropertyName = "title")]
        public String Title { get; protected set; }
    }
}
