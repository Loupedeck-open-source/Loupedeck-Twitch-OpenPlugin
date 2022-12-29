namespace Loupedeck.TwitchPlugin
{
    using Newtonsoft.Json;

    public class CreateMarkerResponse
    {
        [JsonProperty(PropertyName = "data")] 
        public Marker[] Markers { get; protected set; }
    }
}