namespace TwitchLib.Api.Helix.Models.Users.Internal
{
    using System;
    using Newtonsoft.Json;

    public class UserExtension
    {
        [JsonProperty(PropertyName = "id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "version")]
        public String Version { get; protected set; }
        [JsonProperty(PropertyName = "name")]
        public String Name { get; protected set; }
        [JsonProperty(PropertyName = "can_activate")]
        public Boolean CanActivate { get; protected set; }
        [JsonProperty(PropertyName = "type")]
        public String[] Type { get; protected set; }
    }
}
