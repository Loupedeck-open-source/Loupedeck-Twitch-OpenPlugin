namespace Loupedeck.TwitchPlugin.Actions
{
    using System;

    internal class ChatEmoToggleCmd : GenericOnOffSwitch
    {
        public ChatEmoToggleCmd()
                        : base(
                            name: "ToggleEmotesOnly",
                            displayName: "Chat Emotes-Only",
                            description: "Toggles chat emotes-only mode",
                            groupName: "",
                            onStateName: "Emotes-Only chat is On",
                            offStateName: "Emotes-Only chat is Off",
                            offStateImage: "ChatEmotes-OnlyOff.svg",
                            onStateImage: "ChatEmotes-Only.svg")
        {
        }

        protected override void ConnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn)
        {
            TwitchPlugin.Proxy.AppEvtChatEmotesOnlyOff += eventSwitchedOff;
            TwitchPlugin.Proxy.AppEvtChatEmotesOnlyOn += eventSwitchedOn;
        }

        protected override void DisconnectAppEvents(EventHandler<EventArgs> eventSwitchedOff, EventHandler<EventArgs> eventSwitchedOn)
        {
            TwitchPlugin.Proxy.AppEvtChatEmotesOnlyOff -= eventSwitchedOff;
            TwitchPlugin.Proxy.AppEvtChatEmotesOnlyOn -= eventSwitchedOn;
        }

        protected override void RunCommand(TwoStateCommand command)
        {
            switch (command)
            {
                case TwoStateCommand.TurnOff:
                    TwitchPlugin.Proxy.AppEmotesOnlyOff();
                    break;

                case TwoStateCommand.TurnOn:
                    TwitchPlugin.Proxy.AppEmotesOnlyOn();
                    break;

                case TwoStateCommand.Toggle:
                    TwitchPlugin.Proxy.AppToggleEmotesOnly();
                    break;
            }
        }

    }
}
