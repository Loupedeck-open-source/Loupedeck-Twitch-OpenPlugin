namespace TwitchLib.Api.Core.Models.Undocumented.Chatters
{
    using System;
    using Newtonsoft.Json;

    public class ChattersResponse
    {
        [JsonProperty(PropertyName = "chatter_count")]
        public Int32 ChatterCount { get; protected set; }
        [JsonProperty(PropertyName = "chatters")]
        public Chatters Chatters { get; protected set; }
    }
}
