namespace TwitchLib.Api.Core.Models.Undocumented.Comments
{
    using System;
    using Newtonsoft.Json;

    public class Emoticons
    {
        [JsonProperty(PropertyName = "_id")]
        public String Id { get; set; }
        [JsonProperty(PropertyName = "begin")]
        public Int32 Begin { get; set; }
        [JsonProperty(PropertyName = "end")]
        public Int32 End { get; set; }
    }
}