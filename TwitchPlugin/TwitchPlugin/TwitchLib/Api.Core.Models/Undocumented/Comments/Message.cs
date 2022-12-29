namespace TwitchLib.Api.Core.Models.Undocumented.Comments
{
    using System;
    using Newtonsoft.Json;

    public class Message
    {
        [JsonProperty(PropertyName = "body")]
        public String Body { get; set; }
        [JsonProperty(PropertyName = "emoticons")]
        public Emoticons[] Emoticons { get; set; }
        [JsonProperty(PropertyName = "fragments")]
        public Fragment[] Fragments { get; set; }
        [JsonProperty(PropertyName = "is_action")]
        public Boolean IsAction { get; set; }
        [JsonProperty(PropertyName = "user_color")]
        public String UserColor { get; set; }
        [JsonProperty(PropertyName = "user_badges")]
        public UserBadges[] UserBadges { get; set; }
    }
}