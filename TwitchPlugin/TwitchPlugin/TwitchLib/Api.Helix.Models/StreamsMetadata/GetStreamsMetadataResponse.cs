namespace TwitchLib.Api.Helix.Models.StreamsMetadata
{
    using Common;
    using Newtonsoft.Json;

    public class GetStreamsMetadataResponse
    {
        [JsonProperty(PropertyName = "data")]
        public StreamMetadata[] StreamsMetadatas { get; protected set; }
        [JsonProperty(PropertyName = "pagination")]
        public Pagination Pagination { get; protected set; }
    }
}
