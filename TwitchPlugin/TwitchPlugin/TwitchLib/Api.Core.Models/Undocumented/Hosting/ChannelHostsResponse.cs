namespace TwitchLib.Api.Core.Models.Undocumented.Hosting
{
    using Newtonsoft.Json;

    public class ChannelHostsResponse
    {
        [JsonProperty(PropertyName = "hosts")]
        public HostListing[] Hosts { get; protected set; }
    }
}
