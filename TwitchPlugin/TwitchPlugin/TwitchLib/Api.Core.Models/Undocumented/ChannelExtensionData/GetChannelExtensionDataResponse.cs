namespace TwitchLib.Api.Core.Models.Undocumented.ChannelExtensionData
{
    using System;
    using Newtonsoft.Json;

    public class GetChannelExtensionDataResponse
    {
        [JsonProperty(PropertyName = "issued_at")]
        public String IssuedAt { get; protected set; }
        [JsonProperty(PropertyName = "tokens")]
        public ExtToken[] Tokens { get; protected set; }
        [JsonProperty(PropertyName = "installed_extensions")]
        public InstalledExtension[] InstalledExtensions { get; protected set; }
    }
}
