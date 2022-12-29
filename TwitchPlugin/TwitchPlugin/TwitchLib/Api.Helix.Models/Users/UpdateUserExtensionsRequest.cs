namespace TwitchLib.Api.Helix.Models.Users
{
    using System;
    using System.Collections.Generic;
    using Internal;
    using Newtonsoft.Json;

    public class UpdateUserExtensionsRequest
    {
        [JsonProperty(PropertyName = "panel")]
        public Dictionary<String, UserExtensionState> Panel { get; set; }
        [JsonProperty(PropertyName = "component")]
        public Dictionary<String, UserExtensionState> Component { get; set; }
        [JsonProperty(PropertyName = "overlay")]
        public Dictionary<String, UserExtensionState> Overlay { get; set; }
    }
}
