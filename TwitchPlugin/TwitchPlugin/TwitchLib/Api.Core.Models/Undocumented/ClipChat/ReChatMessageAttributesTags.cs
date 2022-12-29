namespace TwitchLib.Api.Core.Models.Undocumented.ClipChat
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ReChatMessageAttributesTags
    {
        [JsonProperty(PropertyName = "badges")]
        public String Badges { get; protected set; }
        [JsonProperty(PropertyName = "color")]
        public String Color { get; protected set; }
        [JsonProperty(PropertyName = "display-name")]
        public String DisplayName { get; protected set; }
        [JsonProperty(PropertyName = "emotes")]
        public Dictionary<String, Int32[][]> Emotes { get; protected set; }
        [JsonProperty(PropertyName = "id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "mod")]
        public Boolean Mod { get; protected set; }
        [JsonProperty(PropertyName = "room-id")]
        public String RoomId { get; protected set; }
        [JsonProperty(PropertyName = "sent-ts")]
        public String SentTs { get; protected set; }
        [JsonProperty(PropertyName = "subscriber")]
        public Boolean Subscriber { get; protected set; }
        [JsonProperty(PropertyName = "tmi-sent-ts")]
        public String TmiSentTs { get; protected set; }
        [JsonProperty(PropertyName = "turbo")]
        public Boolean Turbo { get; protected set; }
        [JsonProperty(PropertyName = "user-id")]
        public String UserId { get; protected set; }
        [JsonProperty(PropertyName = "user-type")]
        public String UserType { get; protected set; }
    }
}
