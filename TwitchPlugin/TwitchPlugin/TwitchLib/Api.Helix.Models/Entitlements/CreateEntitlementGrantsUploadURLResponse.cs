namespace TwitchLib.Api.Helix.Models.Entitlements
{
    using Newtonsoft.Json;

    public class CreateEntitlementGrantsUploadUrlResponse
    {
        [JsonProperty(PropertyName = "data")]
        public UploadUrl[] Data { get; protected set; }
    }
}
