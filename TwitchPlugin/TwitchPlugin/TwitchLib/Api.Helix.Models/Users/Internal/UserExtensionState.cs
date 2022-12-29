namespace TwitchLib.Api.Helix.Models.Users.Internal
{
    using System;
    using Newtonsoft.Json;

    public class UserExtensionState
    {
        [JsonProperty(PropertyName = "active")]
        public Boolean Active { get; protected set; }
        [JsonProperty(PropertyName = "id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "version")]
        public String Version { get; protected set; }

        public UserExtensionState(Boolean active, String id, String version)
        {
            this.Active = active;
            this.Id = id;
            this.Version = version;
        }
    }
}
