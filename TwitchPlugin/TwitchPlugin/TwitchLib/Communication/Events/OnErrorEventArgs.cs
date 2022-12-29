namespace TwitchLib.Communication.Events
{
    using System;

    public class OnErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
    }
}
