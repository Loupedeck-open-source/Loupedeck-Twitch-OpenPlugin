namespace TwitchLib.Api.Helix.Models.Clips.GetClip
{
    using Newtonsoft.Json;

    public class GetClipResponse
    {
        [JsonProperty(PropertyName = "data")]
        public Clip[] Clips { get; protected set; }
    }
}
