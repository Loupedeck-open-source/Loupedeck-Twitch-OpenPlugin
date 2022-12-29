namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    /// <inheritdoc />
    /// <summary>Args representing new subscriber event.</summary>
    public class OnNewSubscriberArgs : EventArgs
    {
        /// <summary>Property representing subscriber object.</summary>
        public Subscriber Subscriber;
        /// <summary>Property representing the Twitch channel this event fired from.</summary>
        public String Channel;
    }
}
