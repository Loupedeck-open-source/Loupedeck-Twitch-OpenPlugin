namespace TwitchLib.Api.Core.Models.Undocumented.ChannelExtensionData
{
    using System;
    using Newtonsoft.Json;

    public class ActivationConfig
    {
        [JsonProperty(PropertyName = "slot")]
        public String Slot { get; protected set; }
        [JsonProperty(PropertyName = "anchor")]
        public String Anchor { get; protected set; }
    }
}
