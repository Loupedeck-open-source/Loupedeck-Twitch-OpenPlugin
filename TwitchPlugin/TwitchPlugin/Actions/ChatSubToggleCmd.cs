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
                            onStateName: "Subscribers-Only chat is On",
                            offStateName: "Subscribers-Only chat is Off",
                            onStateImage: "TwitchSubChat2.png",
                            offStateImage: "TwitchSubChat.png")
        {
        }

        protected override void ConnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn)
        {
            TwitchPlugin.Proxy.AppEvtChatSubscribersOnlyOn += eventSwitchedOn;
            TwitchPlugin.Proxy.AppEvtChatSubscribersOnlyOff += eventSwitchedOff;
        }

        protected override void DisconnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn)
        {
            TwitchPlugin.Proxy.AppEvtChatSubscribersOnlyOn -= eventSwitchedOn;
            TwitchPlugin.Proxy.AppEvtChatSubscribersOnlyOff -= eventSwitchedOff;
        }

        protected override void RunToggle() => TwitchPlugin.Proxy.AppToggleSubscribersOnly();
    }
}
