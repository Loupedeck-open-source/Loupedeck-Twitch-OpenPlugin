namespace TwitchLib.Api.Core.Models.Undocumented.ClipChat
{
    using Newtonsoft.Json;

    public class GetClipChatResponse
    {
        [JsonProperty(PropertyName = "data")]
        public ReChatMessage[] Messages { get; protected set; }
    }
}
