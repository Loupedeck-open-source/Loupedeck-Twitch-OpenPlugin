namespace TwitchLib.Client.Events
{
    using System;

    public class OnLogArgs : EventArgs
    {
        public String BotUsername;
        public String Data;
        public DateTime DateTime;
    }
}
