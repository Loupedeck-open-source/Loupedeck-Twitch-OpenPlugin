namespace Loupedeck.TwitchPlugin.Actions
{
    using System;

    internal class ChatEmoToggleCmd : GenericOnOffSwitch
    {
        public ChatEmoToggleCmd()
                        : base(
                            name: "ToggleEmotesOnly1",
                            displayName: "(new) Chat Emotes-Only",
                            description: "Toggles chat emotes-only mode",
                            groupName: "",
                            offStateName: "Chat Emotes-Only On",
                            onStateName: "Chat Emotes-Only Off",
                            offStateImage: "TwitchEmoteChat.png",
                            onStateImage: "TwitchEmoteChatToggle.png")
        {
        }

        protected override void ConnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn)
        {
            TwitchPlugin.Proxy.AppEvtChatEmotesOnlyOff += eventSwitchedOn;
            TwitchPlugin.Proxy.AppEvtChatEmotesOnlyOn += eventSwitchedOff;
        }

        protected override void DisconnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn)
        {
            TwitchPlugin.Proxy.AppEvtChatEmotesOnlyOff -= eventSwitchedOn;
            TwitchPlugin.Proxy.AppEvtChatEmotesOnlyOn -= eventSwitchedOff;
        }

        protected override void RunToggle() => TwitchPlugin.Proxy.AppToggleEmotesOnly();
    }
}
