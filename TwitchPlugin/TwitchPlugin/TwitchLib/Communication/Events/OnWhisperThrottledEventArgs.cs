namespace TwitchLib.Communication.Events
{
    using System;

    public class OnWhisperThrottledEventArgs : EventArgs
    {
        public String Message { get; set; }
        public Int32 SentWhisperCount { get; set; }
        public TimeSpan Period { get; set; }
        public Int32 AllowedInPeriod { get; set; }
    }
}
