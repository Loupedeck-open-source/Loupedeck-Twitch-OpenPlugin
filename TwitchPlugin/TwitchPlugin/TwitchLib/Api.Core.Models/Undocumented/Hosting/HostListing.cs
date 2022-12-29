namespace TwitchLib.Api.Core.Models.Undocumented.Hosting
{
    using System;
    using Newtonsoft.Json;

    public class HostListing
    {
        [JsonProperty(PropertyName = "host_id")]
        public String HostId { get; protected set; }
        [JsonProperty(PropertyName = "target_id")]
        public String TargetId { get; protected set; }
        [JsonProperty(PropertyName = "host_login")]
        public String HostLogin { get; protected set; }
        [JsonProperty(PropertyName = "target_login")]
        public String TargetLogin { get; protected set; }
        [JsonProperty(PropertyName = "host_display_name")]
        public String HostDisplayName { get; protected set; }
        [JsonProperty(PropertyName = "target_display_name")]
        public String TargetDisplayName { get; protected set; } 
    }
}
