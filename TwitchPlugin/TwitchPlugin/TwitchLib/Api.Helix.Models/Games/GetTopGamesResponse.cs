namespace TwitchLib.Api.Helix.Models.Games
{
    using Common;
    using Newtonsoft.Json;

    public class GetTopGamesResponse
    {
        [JsonProperty(PropertyName = "data")]
        public Game[] Data { get; protected set; }
        [JsonProperty(PropertyName = "pagination")]
        public Pagination Pagination { get; protected set; }
    }
}
