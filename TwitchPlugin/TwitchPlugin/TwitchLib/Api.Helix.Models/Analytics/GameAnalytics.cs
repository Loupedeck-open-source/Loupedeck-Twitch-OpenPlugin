namespace TwitchLib.Api.Helix.Models.Analytics
{
    using System;
    using Newtonsoft.Json;

    public class GameAnalytics
    {
        [JsonProperty(PropertyName = "game_id")]
        public String GameId { get; protected set; }
        [JsonProperty(PropertyName = "URL")]
        public String Url { get; protected set; }
        [JsonProperty(PropertyName = "type")]
        public String Type { get; protected set; }
        [JsonProperty(PropertyName = "date_range")]
        public Common.DateRange DateRange { get; protected set; }
    }
}
