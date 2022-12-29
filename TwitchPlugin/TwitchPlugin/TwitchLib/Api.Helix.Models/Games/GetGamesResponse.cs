namespace TwitchLib.Api.Helix.Models.Games
{
    using Newtonsoft.Json;

    public class GetGamesResponse
    {
        [JsonProperty(PropertyName = "data")]
        public Game[] Games { get; protected set; }
    }
}
