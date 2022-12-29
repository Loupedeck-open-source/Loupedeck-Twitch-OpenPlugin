namespace TwitchLib.Client.Events
{
    using System;

    public class OnUnaccountedForArgs : EventArgs
    {
        public String RawIRC { get; set; }
        public String Location { get; set; }
        public String BotUsername { get; set; } // may not be available
        public String Channel { get; set; } // may not be available
    }
}
