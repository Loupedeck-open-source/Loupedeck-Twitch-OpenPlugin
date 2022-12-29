namespace TwitchLib.Api.Core.Models.Undocumented.TwitchPrimeOffers
{
    using Newtonsoft.Json;

    public class TwitchPrimeOffers
    {
        [JsonProperty(PropertyName = "offers")]
        public Offer[] Offers { get; protected set; }
    }
}
