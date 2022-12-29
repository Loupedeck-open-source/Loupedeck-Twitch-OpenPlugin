namespace TwitchLib.Communication.Events
{
    using System;

    public class OnMessageThrottledEventArgs : EventArgs
    {
        public String Message { get; set; }
        public Int32 SentMessageCount { get; set; }
        public TimeSpan Period { get; set; }
        public Int32 AllowedInPeriod { get; set; }
    }
}
