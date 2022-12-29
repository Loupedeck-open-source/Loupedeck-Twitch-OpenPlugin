namespace TwitchLib.Api.Core.Models.Undocumented.RecentMessages
{
    using System;
    using Newtonsoft.Json;

    public class RecentMessagesResponse
    {
        [JsonProperty(PropertyName = "messages")]
        public String[] Messages { get; protected set; }
    }
}
