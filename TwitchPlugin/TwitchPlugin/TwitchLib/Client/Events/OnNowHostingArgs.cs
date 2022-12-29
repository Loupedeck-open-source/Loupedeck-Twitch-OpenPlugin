namespace TwitchLib.Client.Events
{
    using System;

    /// <inheritdoc />
    /// <summary>Args representing the detected hosted channel.</summary>
    public class OnNowHostingArgs : EventArgs
    {
        /// <summary>Property the channel that received the event.</summary>
        public String Channel;
        /// <summary>Property representing channel that is being hosted.</summary>
        public String HostedChannel;
    }
}
