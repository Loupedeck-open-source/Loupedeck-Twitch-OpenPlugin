namespace Loupedeck.TwitchPlugin.Actions
{
    using System;

    internal class ShieldModeToggleCmd : GenericOnOffSwitch
    {
        public ShieldModeToggleCmd()
                        : base(
                            name: "ToggleShieldmode",
                            displayName: "Chat Shield Mode",
                            description: "Toggles chat shield mode",
                            groupName: "",
                            onStateName: "Shield Mode is On",
                            offStateName: "Shield Mode is Off",
                            offStateImage: "TwitchShieldModeOff.png",
                            onStateImage: "TwitchShieldModeOn.png")
        {
        }

        protected override void ConnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn)
        {
            TwitchPlugin.Proxy.AppEvtShieldModeOff += eventSwitchedOff;
            TwitchPlugin.Proxy.AppEvtShieldModeOn += eventSwitchedOn;
        }

        protected override void DisconnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn)
        {
            TwitchPlugin.Proxy.AppEvtShieldModeOff -= eventSwitchedOff;
            TwitchPlugin.Proxy.AppEvtShieldModeOn -= eventSwitchedOn;
        }

        protected override void RunCommand(TwoStateCommand command)
        {
            switch (command)
            {
                case TwoStateCommand.TurnOff:
                    TwitchPlugin.Proxy.AppShieldModeOff();
                    break;

                case TwoStateCommand.TurnOn:
                    TwitchPlugin.Proxy.AppShieldModeOn();
                    break;

                case TwoStateCommand.Toggle:
                    TwitchPlugin.Proxy.AppToggleShieldMode();
                    break;
            }
        }

    }
}
