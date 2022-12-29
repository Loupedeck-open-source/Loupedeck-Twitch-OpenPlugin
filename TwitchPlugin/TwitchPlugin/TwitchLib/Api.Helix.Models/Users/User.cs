namespace TwitchLib.Api.Helix.Models.Users
{
    using System;
    using Newtonsoft.Json;

    public class User
    {
        [JsonProperty(PropertyName = "id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "login")]
        public String Login { get; protected set; }
        [JsonProperty(PropertyName = "display_name")]
        public String DisplayName { get; protected set; }
        [JsonProperty(PropertyName = "type")]
        public String Type { get; protected set; }
        [JsonProperty(PropertyName = "broadcaster_type")]
        public String BroadcasterType { get; protected set; }
        [JsonProperty(PropertyName = "description")]
        public String Description { get; protected set; }
        [JsonProperty(PropertyName = "profile_image_url")]
        public String ProfileImageUrl { get; protected set; }
        [JsonProperty(PropertyName = "offline_image_url")]
        public String OfflineImageUrl { get; protected set; }
        [JsonProperty(PropertyName = "view_count")]
        public Int64 ViewCount { get; protected set; }
        [JsonProperty(PropertyName = "email")]
        public String Email { get; protected set; }
    }
}
