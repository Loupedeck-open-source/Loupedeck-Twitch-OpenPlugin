namespace TwitchLib.Api.Core.Models.Undocumented.TwitchPrimeOffers
{
    using System;
    using Newtonsoft.Json;

    public class Offer
    {
        [JsonProperty(PropertyName = "applicableGame")]
        public String ApplicableGame { get; protected set; }
        [JsonProperty(PropertyName = "assets")]
        public Asset[] Assets { get; protected set; }
        [JsonProperty(PropertyName = "contentCategories")]
        public String[] ContentCategories { get; protected set; }
        [JsonProperty(PropertyName = "contentClaimInstructions")]
        public String ContentClaimInstruction { get; protected set; }
        [JsonProperty(PropertyName = "contentDeliveryMethod")]
        public String ContentDeliveryMethod { get; protected set; }
        [JsonProperty(PropertyName = "endTime")]
        public DateTime EndTime { get; protected set; }
        [JsonProperty(PropertyName = "offerDescription")]
        public String OfferDescription { get; protected set; }
        [JsonProperty(PropertyName = "offerId")]
        public String OfferId { get; protected set; }
        [JsonProperty(PropertyName = "offerTitle")]
        public String OfferTitle { get; protected set; }
        [JsonProperty(PropertyName = "priority")]
        public Int32 Priority { get; protected set; }
        [JsonProperty(PropertyName = "publisherName")]
        public String PublisherName { get; protected set; }
        [JsonProperty(PropertyName = "startTime")]
        public DateTime StartTime { get; protected set; }
    }
}
