namespace TwitchLib.Api.Helix.Models.Users
{
    using Internal;
    using Newtonsoft.Json;

    public class GetUserActiveExtensionsResponse
    {
        [JsonProperty(PropertyName = "data")]
        public ActiveExtensions Data { get; protected set; }
    }
}
