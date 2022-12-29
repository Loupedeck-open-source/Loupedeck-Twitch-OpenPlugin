namespace TwitchLib.Api.Core.Models.Undocumented.ClipChat
{
    using System;
    using Newtonsoft.Json;

    public class ReChatMessage
    {
        [JsonProperty(PropertyName = "type")]
        public String Type { get; protected set; }
        [JsonProperty(PropertyName = "id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "attributes")]
        public ReChatMessageAttributes Attributes { get; protected set; }
    }
}
