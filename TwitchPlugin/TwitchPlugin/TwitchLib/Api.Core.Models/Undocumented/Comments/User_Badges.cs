namespace TwitchLib.Api.Core.Models.Undocumented.Comments
{
    using System;
    using Newtonsoft.Json;

    public class UserBadges
    {
        [JsonProperty(PropertyName = "_id")]
        public String Id { get; set; }
        [JsonProperty(PropertyName = "version")]
        public String Version { get; set; }
    }
}