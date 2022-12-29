namespace TwitchLib.Api.Helix.Models.Analytics
{
    using System;
    using Newtonsoft.Json;

    public class ExtensionAnalytics
    {
        [JsonProperty(PropertyName = "extension_id")]
        public String ExtensionId { get; protected set; }
        [JsonProperty(PropertyName = "URL")]
        public String Url { get; protected set; }
    }
}
