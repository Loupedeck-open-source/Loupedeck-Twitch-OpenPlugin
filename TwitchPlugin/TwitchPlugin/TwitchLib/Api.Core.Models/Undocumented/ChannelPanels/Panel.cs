namespace TwitchLib.Api.Core.Models.Undocumented.ChannelPanels
{
    using System;
    using Newtonsoft.Json;

    public class Panel
    {
        [JsonProperty(PropertyName = "_id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "display_order")]
        public Int32 DisplayOrder { get; protected set; }
        [JsonProperty(PropertyName = "default")]
        public String Kind { get; protected set; }
        [JsonProperty(PropertyName = "html_description")]
        public String HtmlDescription { get; protected set; }
        [JsonProperty(PropertyName = "user_id")]
        public String UserId { get; protected set; }
        [JsonProperty(PropertyName = "data")]
        public Data Data { get; protected set; }
        [JsonProperty(PropertyName = "channel")]
        public String Channel { get; protected set; }
    }
}
