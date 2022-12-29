namespace TwitchLib.Api.Helix.Models.Videos
{
    using Common;
    using Newtonsoft.Json;

    public class GetVideosResponse
    {
        [JsonProperty(PropertyName = "data")]
        public Video[] Videos { get; protected set; }
        [JsonProperty(PropertyName = "pagination")]
        public Pagination Pagination { get; protected set; }
    }
}
