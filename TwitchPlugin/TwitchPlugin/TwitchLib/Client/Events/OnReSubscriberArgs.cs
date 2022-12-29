namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    /// <inheritdoc />
    /// <summary>Args representing resubscriber event.</summary>
    public class OnReSubscriberArgs : EventArgs
    {
        /// <summary>Property representing resubscriber object.</summary>
        public ReSubscriber ReSubscriber;
        /// <summary>Property representing the Twitch channel this event fired from.</summary>
        public String Channel;
    }
}
