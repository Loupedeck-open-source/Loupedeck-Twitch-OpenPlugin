namespace TwitchLib.Api.Core.Models.Undocumented.Comments
{
    using System;
    using Newtonsoft.Json;

    public class Emoticon
    {
        [JsonProperty(PropertyName = "emoticon_id")]
        public String EmoticonId { get; set; }
        [JsonProperty(PropertyName = "emoticon_set_id")]
        public String EmoticonSetId { get; set; }
    }
}