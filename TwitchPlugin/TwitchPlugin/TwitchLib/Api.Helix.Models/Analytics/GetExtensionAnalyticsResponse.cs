namespace TwitchLib.Api.Helix.Models.Analytics
{
    using Newtonsoft.Json;

    public class GetExtensionAnalyticsResponse
    {
        [JsonProperty(PropertyName = "data")]
        public ExtensionAnalytics[] Data { get; protected set; }
    }
}
