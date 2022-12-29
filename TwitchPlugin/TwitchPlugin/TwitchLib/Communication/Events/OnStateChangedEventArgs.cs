namespace TwitchLib.Communication.Events
{
    using System;

    public class OnStateChangedEventArgs : EventArgs
    {
        public Boolean IsConnected;
        public Boolean WasConnected;
    }
}
