namespace TwitchLib.Api.Helix.Models.Users.Internal
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ActiveExtensions
    {
        [JsonProperty(PropertyName = "panel")]
        public Dictionary<String, UserActiveExtension> Panel { get; protected set; }
        [JsonProperty(PropertyName = "overlay")]
        public Dictionary<String, UserActiveExtension> Overlay { get; protected set; }
        [JsonProperty(PropertyName = "component")]
        public Dictionary<String, UserActiveExtension> Component { get; protected set; }
    }
}
