namespace TwitchLib.Api.Core.Models.Undocumented.ChannelExtensionData
{
    using System;
    using Newtonsoft.Json;

    public class InstallationStatus
    {
        [JsonProperty(PropertyName = "extension_id")]
        public String ExtensionId { get; protected set; }
        [JsonProperty(PropertyName = "activation_config")]
        public ActivationConfig ActivationConfig { get; protected set; }
        [JsonProperty(PropertyName = "activation_state")]
        public String ActivationState { get; protected set; }
        [JsonProperty(PropertyName = "can_activate")]
        public Boolean CanActivate { get; protected set; }
    }
}
