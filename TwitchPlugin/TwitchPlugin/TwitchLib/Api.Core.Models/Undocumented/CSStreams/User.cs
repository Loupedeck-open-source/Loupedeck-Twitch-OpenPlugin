namespace TwitchLib.Api.Core.Models.Undocumented.CSStreams
{
    using System;
    using Newtonsoft.Json;

    public class User
    {
        [JsonProperty(PropertyName = "mature")]
        public Boolean Mature { get; protected set; }
        [JsonProperty(PropertyName = "status")]
        public String Status { get; protected set; }
        [JsonProperty(PropertyName = "broadcaster_language")]
        public String BroadcasterLanguage { get; protected set; }
        [JsonProperty(PropertyName = "display_name")]
        public String DisplayName { get; protected set; }
        [JsonProperty(PropertyName = "game")]
        public String Game { get; protected set; }
        [JsonProperty(PropertyName = "localized_game")]
        public LocalizedGame LocalizedGame { get; protected set; }
        [JsonProperty(PropertyName = "_id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "name")]
        public String Name { get; protected set; }
        [JsonProperty(PropertyName = "bio")]
        public String Bio { get; protected set; }
        [JsonProperty(PropertyName = "partner")]
        public Boolean Partner { get; protected set; }
        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; protected set; }
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime UpdatedAt { get; protected set; }
        [JsonProperty(PropertyName = "delay")]
        public String Delay { get; protected set; }
        [JsonProperty(PropertyName = "prerolls")]
        public Boolean Prerolls { get; protected set; }
        [JsonProperty(PropertyName = "postrolls")]
        public Boolean Postrolls { get; protected set; }
        [JsonProperty(PropertyName = "primary_team_name")]
        public String PrimaryTeamName { get; protected set; }
        [JsonProperty(PropertyName = "primary_team_display_name")]
        public String PrimaryTeamDisplayName { get; protected set; }
        [JsonProperty(PropertyName = "logo")]
        public String Logo { get; protected set; }
        [JsonProperty(PropertyName = "banner")]
        public String Banner { get; protected set; }
        [JsonProperty(PropertyName = "video_banner")]
        public String VideoBanner { get; protected set; }
        [JsonProperty(PropertyName = "background")]
        public String Background { get; protected set; }
        [JsonProperty(PropertyName = "profile_banner")]
        public String ProfileBanner { get; protected set; }
        [JsonProperty(PropertyName = "profile_banner_background_color")]
        public String ProfileBannerBackgroundColor { get; protected set; }
        [JsonProperty(PropertyName = "url")]
        public String Url { get; protected set; }
        [JsonProperty(PropertyName = "views")]
        public Int32 Views { get; protected set; }
        [JsonProperty(PropertyName = "followers")]
        public Int32 Followers { get; protected set; }
    }
}
