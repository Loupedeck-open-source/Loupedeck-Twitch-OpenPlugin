namespace TwitchLib.Api.Core.Models.Undocumented.ChannelExtensionData
{
    using System;
    using Newtonsoft.Json;

    public class View
    {
        [JsonProperty(PropertyName = "viewer_url")]
        public String ViewerUrl { get; protected set; }
    }
}
