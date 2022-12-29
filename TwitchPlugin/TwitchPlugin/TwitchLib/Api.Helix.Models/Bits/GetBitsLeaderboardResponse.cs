namespace TwitchLib.Api.Helix.Models.Bits
{
    using System;
    using Newtonsoft.Json;

    public class GetBitsLeaderboardResponse
    {
        [JsonProperty(PropertyName = "data")]
        public Listing[] Listings { get; protected set; }
        [JsonProperty(PropertyName = "date_range")]
        public DateRange DateRange { get; protected set; }
        [JsonProperty(PropertyName = "total")]
        public Int32 Total { get; protected set; }
    }
}
