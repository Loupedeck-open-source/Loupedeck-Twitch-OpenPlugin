namespace Loupedeck.TwitchPlugin.Actions
{
    using System;

    internal class ChatSubToggleCmd : GenericOnOffSwitch
    {
        public ChatSubToggleCmd()
                        : base(
                            name: "ToggleSubscribersOnly1",
                            displayName: "(new) Chat Subscribers-Only",
                            description: "Toggles chat Subscribers-only mode",
                            groupName: "",
                            offStateName: "Chat Subscribers-Only On",
                            onStateName: "Chat Subscribers-Only Off",
                            offStateImage: "TwitchSubChat2.png",
                            onStateImage: "TwitchSubChat.png")
        {
        }

        protected override void ConnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn)
        {
            TwitchPlugin.Proxy.AppEvtChatSubscribersOnlyOff += eventSwitchedOn;
            TwitchPlugin.Proxy.AppEvtChatSubscribersOnlyOn += eventSwitchedOff;
        }

        protected override void DisconnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn)
        {
            TwitchPlugin.Proxy.AppEvtChatSubscribersOnlyOff -= eventSwitchedOn;
            TwitchPlugin.Proxy.AppEvtChatSubscribersOnlyOn -= eventSwitchedOff;
        }

        protected override void RunToggle() => TwitchPlugin.Proxy.AppToggleSubscribersOnly();
    }
}
