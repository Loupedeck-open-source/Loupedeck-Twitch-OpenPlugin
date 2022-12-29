namespace TwitchLib.Api.Helix.Models.Users
{
    using System;
    using Common;
    using Newtonsoft.Json;

    public class GetUsersFollowsResponse
    {
        [JsonProperty(PropertyName = "data")]
        public Follow[] Follows { get; protected set; }
        [JsonProperty(PropertyName = "pagination")]
        public Pagination Pagination { get; protected set; }
        [JsonProperty(PropertyName = "total")]
        public Int64 TotalFollows { get; protected set; }
    }
}
