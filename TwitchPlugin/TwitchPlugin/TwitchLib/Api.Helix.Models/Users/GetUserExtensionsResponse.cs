namespace TwitchLib.Api.Helix.Models.Users
{
    using Internal;
    using Newtonsoft.Json;

    public class GetUserExtensionsResponse
    {
        [JsonProperty(PropertyName = "data")]
        public UserExtension[] Users { get; protected set; }
    }
}
