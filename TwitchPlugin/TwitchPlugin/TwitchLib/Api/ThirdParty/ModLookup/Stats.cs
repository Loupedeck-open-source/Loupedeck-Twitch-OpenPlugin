namespace TwitchLib.Api.ThirdParty.ModLookup
{
    using System;
    using Newtonsoft.Json;

    public class Stats
    {
        [JsonProperty(PropertyName = "relations")]
        public Int32 Relations { get; protected set; }
        [JsonProperty(PropertyName = "channels_total")]
        public Int32 ChannelsTotal { get; protected set; }
        [JsonProperty(PropertyName = "users")]
        public Int32 Users { get; protected set; }
        [JsonProperty(PropertyName = "channels_no_mods")]
        public Int32 ChannelsNoMods { get; protected set; }
        [JsonProperty(PropertyName = "channels_only_broadcaster")]
        public Int32 ChannelsOnlyBroadcaster { get; protected set; }
    }
}
