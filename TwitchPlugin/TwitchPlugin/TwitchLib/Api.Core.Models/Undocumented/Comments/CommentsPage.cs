namespace TwitchLib.Api.Core.Models.Undocumented.Comments
{
    using System;
    using Newtonsoft.Json;

    public class CommentsPage
    {
        [JsonProperty(PropertyName = "comments")]
        public Comment[] Comments { get; set; }
        [JsonProperty(PropertyName = "_prev")]
        public String Prev { get; set; }
        [JsonProperty(PropertyName = "_next")]
        public String Next { get; set; }
    }
}
