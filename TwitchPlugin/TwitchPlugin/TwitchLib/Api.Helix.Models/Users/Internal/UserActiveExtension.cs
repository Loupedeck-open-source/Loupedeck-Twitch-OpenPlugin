namespace TwitchLib.Api.Helix.Models.Users.Internal
{
    using System;
    using Newtonsoft.Json;

    public class UserActiveExtension
    {
        [JsonProperty(PropertyName = "active")]
        public Boolean Active { get; protected set; }
        [JsonProperty(PropertyName = "id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "version")]
        public String Version { get; protected set; }
        [JsonProperty(PropertyName = "name")]
        public String Name { get; protected set; }
        [JsonProperty(PropertyName = "x")]
        public Int32 X { get; protected set; }
        [JsonProperty(PropertyName = "y")]
        public Int32 Y { get; protected set; }
    }
}
