namespace TwitchLib.Api.Core.Models.Undocumented.Comments
{
    using System;
    using Newtonsoft.Json;

    public class Commenter
    {
        [JsonProperty(PropertyName = "display_name")]
        public String DisplayName { get; set; }
        [JsonProperty(PropertyName = "_id")]
        public String Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }
        [JsonProperty(PropertyName = "type")]
        public String Type { get; set; }
        [JsonProperty(PropertyName = "bio")]
        public String Bio { get; set; }
        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty(PropertyName = "logo")]
        public String Logo { get; set; }
    }
}