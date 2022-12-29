namespace Loupedeck.TwitchPlugin.Actions
{
    using System;

    internal class ChatFlwToggleCmd : GenericOnOffSwitch
    {
        //FIXME: Add Parameters for Minutes
        public ChatFlwToggleCmd()
                        : base(
                            name: "ToggleFollowersOnlyList1",
                            displayName: "Chat Followers-Only",
                            description: "Toggles chat emotes-only mode",
                            groupName: "",
                            offStateName: "Chat Followers-Only On",
                            onStateName: "Chat Followers-Only Off",
                            offStateImage: "TwitchFollowerChat.png",
                            onStateImage: "TwitchFollowerChatToggle.png")
        {
        }

        protected override void ConnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn)
        {
            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOff += eventSwitchedOn;
            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOn += eventSwitchedOff;
        }

        protected override void DisconnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn)
        {
            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOff -= eventSwitchedOn;
            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOn -= eventSwitchedOff;
        }

        protected override void RunToggle() => TwitchPlugin.Proxy.AppToggleFollowersOnly();
    }
}
