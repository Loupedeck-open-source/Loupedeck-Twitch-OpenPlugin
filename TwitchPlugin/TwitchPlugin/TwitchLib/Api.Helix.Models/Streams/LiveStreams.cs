namespace TwitchLib.Api.Helix.Models.Streams
{
    using System;
    using Newtonsoft.Json;

    public class LiveStreams
    {
        #region Total
        [JsonProperty(PropertyName = "_total")]
        public Int32 Total { get; protected set; }
        #endregion
        #region Streams
        [JsonProperty(PropertyName = "streams")]
        public Stream[] Streams { get; protected set; }
        #endregion
    }
}
