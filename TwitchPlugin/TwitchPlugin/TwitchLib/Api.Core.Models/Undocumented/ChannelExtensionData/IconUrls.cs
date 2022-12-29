namespace TwitchLib.Api.Core.Models.Undocumented.ChannelExtensionData
{
    using System;
    using Newtonsoft.Json;

    public class IconUrls
    {
        [JsonProperty(PropertyName = "100x100")]
        public String Url100x100 { get; protected set; }
    }
}
