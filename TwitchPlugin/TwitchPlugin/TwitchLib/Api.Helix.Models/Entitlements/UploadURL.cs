namespace TwitchLib.Api.Helix.Models.Entitlements
{
    using System;
    using Newtonsoft.Json;

    public class UploadUrl
    {
        [JsonProperty(PropertyName = "url")]
        public String Url { get; protected set; }
    }
}
