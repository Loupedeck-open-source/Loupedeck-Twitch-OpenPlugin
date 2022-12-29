namespace TwitchLib.Api.Core.Models.Undocumented.Chatters
{
    using System;
    using Newtonsoft.Json;

    public class Chatters
    {
        [JsonProperty(PropertyName = "moderators")]
        public String[] Moderators { get; protected set; }
        [JsonProperty(PropertyName = "staff")]
        public String[] Staff { get; protected set; }
        [JsonProperty(PropertyName = "admins")]
        public String[] Admins { get; protected set; }
        [JsonProperty(PropertyName = "global_mods")]
        public String[] GlobalMods { get; protected set; }
        [JsonProperty(PropertyName = "viewers")]
        public String[] Viewers { get; protected set; }
    }
}
