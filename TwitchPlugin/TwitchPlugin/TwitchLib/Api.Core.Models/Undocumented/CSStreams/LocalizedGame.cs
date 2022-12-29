namespace TwitchLib.Api.Core.Models.Undocumented.CSStreams
{
    using System;
    using Newtonsoft.Json;

    public class LocalizedGame
    {
        [JsonProperty(PropertyName = "name")]
        public String Name { get; protected set; }
        [JsonProperty(PropertyName = "popularity")]
        public Int32 Popularity { get; protected set; }
        [JsonProperty(PropertyName = "_id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "giantbomb_id")]
        public String GiantbombId { get; protected set; }
        [JsonProperty(PropertyName = "box")]
        public Box Box { get; protected set; }
        [JsonProperty(PropertyName = "logo")]
        public Logo Logo { get; protected set; }
        [JsonProperty(PropertyName = "localized_name")]
        public String LocalizedName { get; protected set; }
        [JsonProperty(PropertyName = "locale")]
        public String Locale { get; protected set; }
    }
}
