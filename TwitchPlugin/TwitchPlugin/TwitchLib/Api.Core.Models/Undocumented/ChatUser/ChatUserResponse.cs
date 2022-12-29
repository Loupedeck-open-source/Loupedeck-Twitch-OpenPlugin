namespace TwitchLib.Api.Core.Models.Undocumented.ChatUser
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ChatUserResponse
    {
        [JsonProperty(PropertyName = "_id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "login")]
        public String Login { get; protected set; }
        [JsonProperty(PropertyName = "display_name")]
        public String DisplayName { get; protected set; }
        [JsonProperty(PropertyName = "color")]
        public String Color { get; protected set; }
        [JsonProperty(PropertyName = "is_verified_bot")]
        public Boolean IsVerifiedBot { get; protected set; }
        [JsonProperty(PropertyName = "badges")]
        public KeyValuePair<String, String>[] Badges { get; protected set; }
    }
}
