namespace TwitchLib.Api.Helix.Models.Clips.CreateClip
{
    using System;
    using Newtonsoft.Json;

    public class CreatedClip
    {
        [JsonProperty(PropertyName = "id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "edit_url")]
        public String EditUrl { get; protected set; }
    }
}
