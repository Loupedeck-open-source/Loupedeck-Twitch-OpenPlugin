namespace TwitchLib.Api.Core.Models.Undocumented.ChannelExtensionData
{
    using System;
    using Newtonsoft.Json;

    public class Extension
    {
        [JsonProperty(PropertyName = "ud")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "state")]
        public String State { get; protected set; }
        [JsonProperty(PropertyName = "version")]
        public String Version { get; protected set; }
        [JsonProperty(PropertyName = "anchor")]
        public String Anchor { get; protected set; }
        [JsonProperty(PropertyName = "panel_height")]
        public Int32 PanelHeight { get; protected set; }
        [JsonProperty(PropertyName = "author_name")]
        public String AuthorName { get; protected set; }
        [JsonProperty(PropertyName = "support_email")]
        public String SupportEmail { get; protected set; }
        [JsonProperty(PropertyName = "name")]
        public String Name { get; protected set; }
        [JsonProperty(PropertyName = "description")]
        public String Description { get; protected set; }
        [JsonProperty(PropertyName = "summary")]
        public String Summary { get; protected set; }
        [JsonProperty(PropertyName = "viewer_url")]
        public String ViewerUrl { get; protected set; }
        [JsonProperty(PropertyName = "viewer_urls")]
        public ViewerUrls ViewerUrls { get; protected set; }
        [JsonProperty(PropertyName = "views")]
        public Views Views { get; protected set; }
        [JsonProperty(PropertyName = "config_url")]
        public String ConfigUrl { get; protected set; }
        [JsonProperty(PropertyName = "live_config_url")]
        public String LiveConfigUrl { get; protected set; }
        [JsonProperty(PropertyName = "icon_url")]
        public String IconUrl { get; protected set; }
        [JsonProperty(PropertyName = "icon_urls")]
        public IconUrls IconUrls { get; protected set; }
        [JsonProperty(PropertyName = "screenshot_urls")]
        public String[] ScreenshotUrls { get; protected set; }
        [JsonProperty(PropertyName = "installation_count")]
        public Int32 InstallationCount { get; protected set; }
        [JsonProperty(PropertyName = "can_install")]
        public Boolean CanInstall { get; protected set; }
        [JsonProperty(PropertyName = "whitelisted_panel_urls")]
        public String[] WhitelistedPanelUrls { get; protected set; }
        [JsonProperty(PropertyName = "whitelisted_config_urls")]
        public String[] WhitelistedConfigUrls { get; protected set; }
        [JsonProperty(PropertyName = "eula_tos_url")]
        public String EulaTosUrl { get; protected set; }
        [JsonProperty(PropertyName = "privacy_policy_url")]
        public String PrivacyPolicyUrl { get; protected set; }
        [JsonProperty(PropertyName = "request_identity_link")]
        public Boolean RequestIdentityLink { get; protected set; }
        [JsonProperty(PropertyName = "vendor_code")]
        public String VendorCode { get; protected set; }
        [JsonProperty(PropertyName = "sku")]
        public String SKU { get; protected set; }
        [JsonProperty(PropertyName = "bits_enabled")]
        public Boolean BitsEnabled { get; protected set; }
    }
}
