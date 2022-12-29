namespace TwitchLib.Api.Helix.Models.Common
{
    using System;
    using Newtonsoft.Json;

    public class Pagination
    {
        [JsonProperty(PropertyName = "cursor")]
        public String Cursor { get; protected set; }
    }
}
