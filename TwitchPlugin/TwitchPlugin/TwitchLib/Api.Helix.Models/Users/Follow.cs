namespace TwitchLib.Api.Helix.Models.Users
{
    using System;
    using Newtonsoft.Json;

    public class Follow
    {
        [JsonProperty(PropertyName = "from_id")]
        public String FromUserId { get; protected set; }
        [JsonProperty(PropertyName = "to_id")]
        public String ToUserId { get; protected set; }
        [JsonProperty(PropertyName = "followed_at")]
        public DateTime FollowedAt { get; protected set; }
    }
}
