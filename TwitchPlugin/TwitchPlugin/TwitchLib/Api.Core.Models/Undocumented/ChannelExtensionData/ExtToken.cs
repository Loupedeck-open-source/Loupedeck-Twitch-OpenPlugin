namespace TwitchLib.Api.Core.Models.Undocumented.ChannelExtensionData
{
    using System;
    using Newtonsoft.Json;

    public class ExtToken
    {
        [JsonProperty(PropertyName = "extension_id")]
        public String ExtensionId { get; protected set; }
        [JsonProperty(PropertyName = "token")]
        public String Token { get; protected set; }
    }
}
