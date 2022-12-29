namespace TwitchLib.Communication.Events
{
    using System;

    public class OnSendFailedEventArgs : EventArgs
    {
        public String Data;
        public Exception Exception;
    }
}
