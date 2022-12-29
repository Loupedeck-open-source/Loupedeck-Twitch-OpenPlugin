namespace TwitchLib.Api.Core.Models.Undocumented.ChatProperties
{
    using System;
    using Newtonsoft.Json;

    public class ChatProperties
    {
        [JsonProperty(PropertyName = "_id")]
        public String Id { get; protected set; }
        [JsonProperty(PropertyName = "hide_chat_links")]
        public Boolean HideChatLinks { get; protected set; }
        [JsonProperty(PropertyName = "chat_delay_duration")]
        public Int32 ChatDelayDuration { get; protected set; }
        [JsonProperty(PropertyName = "chat_rules")]
        public String[] ChatRules { get; protected set; }
        [JsonProperty(PropertyName = "twitchbot_rule_id")]
        public Int32 TwitchbotRuleId { get; protected set; }
        [JsonProperty(PropertyName = "devchat")]
        public Boolean DevChat { get; protected set; }
        [JsonProperty(PropertyName = "game")]
        public String Game { get; protected set; }
        [JsonProperty(PropertyName = "subsonly")]
        public Boolean SubsOnly { get; protected set; }
        [JsonProperty(PropertyName = "chat_servers")]
        public String[] ChatServers { get; protected set; }
        [JsonProperty(PropertyName = "web_socket_servers")]
        public String[] WebSocketServers { get; protected set; }
        [JsonProperty(PropertyName = "web_socket_pct")]
        public Double WebSocketPct { get; protected set; }
        [JsonProperty(PropertyName = "darklaunch_pct")]
        public Double DarkLaunchPct { get; protected set; }
        [JsonProperty(PropertyName = "available_chat_notification_tokens")]
        public String[] AvailableChatNotificationTokens { get; protected set; }
        [JsonProperty(PropertyName = "sce_title_preset_text_1")]
        public String SceTitlePresetText1 { get; protected set; }
        [JsonProperty(PropertyName = "sce_title_preset_text_2")]
        public String SceTitlePresetText2 { get; protected set; }
        [JsonProperty(PropertyName = "sce_title_preset_text_3")]
        public String SceTitlePresetText3 { get; protected set; }
        [JsonProperty(PropertyName = "sce_title_preset_text_4")]
        public String SceTitlePresetText4 { get; protected set; }
        [JsonProperty(PropertyName = "sce_title_preset_text_5")]
        public String SceTitlePresetText5 { get; protected set; }
    }
}
