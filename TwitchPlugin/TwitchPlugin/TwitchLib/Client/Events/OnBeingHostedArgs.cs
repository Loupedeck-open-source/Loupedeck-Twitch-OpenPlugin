namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    /// <inheritdoc />
    /// <summary>Args representing an event where another channel has started hosting the broadcaster's channel.</summary>
    public class OnBeingHostedArgs : EventArgs
    {
        /// <summary>Property representing the Host notification</summary>
        public BeingHostedNotification BeingHostedNotification;
    }
}
