namespace TwitchLib.Api.Core.Models.Undocumented.Comments
{
    using System;
    using Newtonsoft.Json;

    public class Fragment
    {
        [JsonProperty(PropertyName = "text")]
        public String Text { get; set; }
        [JsonProperty(PropertyName = "emoticon")]
        public Emoticon Emoticon { get; set; }
    }
}