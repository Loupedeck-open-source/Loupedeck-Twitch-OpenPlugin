namespace TwitchLib.Api.Core.Models.Undocumented.CSStreams
{
    using System;
    using Newtonsoft.Json;

    public class CSStreams
    {
        [JsonProperty(PropertyName = "_total")]
        public Int32 Total { get; protected set; }
        [JsonProperty(PropertyName = "streams")]
        public CSStream[] Streams { get; protected set; }
    }
}
