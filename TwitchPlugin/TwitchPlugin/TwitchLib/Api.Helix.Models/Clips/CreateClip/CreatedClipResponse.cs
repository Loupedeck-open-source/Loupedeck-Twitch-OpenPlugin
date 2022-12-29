namespace TwitchLib.Api.Helix.Models.Clips.CreateClip
{
    using Newtonsoft.Json;

    public class CreatedClipResponse
    {
        [JsonProperty(PropertyName = "data")]
        public CreatedClip[] CreatedClips { get; protected set; }
    }
}
