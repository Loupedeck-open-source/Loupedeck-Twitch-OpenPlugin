namespace TwitchLib.Api.Core.Models.Undocumented.ChannelExtensionData
{
    using System;
    using Newtonsoft.Json;

    public class ViewerUrls
    {
        [JsonProperty(PropertyName = "video_overlay")]
        public String VideoOverlay { get; protected set; }
    }
}
