namespace TwitchLib.Api.Helix.Models.Webhooks
{
    using System;
    using Common;
    using Newtonsoft.Json;

    public class GetWebhookSubscriptionsResponse
    {
        [JsonProperty(PropertyName = "total")]
        public Int32 Total { get; protected set; }
        [JsonProperty(PropertyName = "data")]
        public Subscription[] Subscriptions { get; protected set; }
        [JsonProperty(PropertyName = "pagination")]
        public Pagination Pagination { get; protected set; }
    }
}
