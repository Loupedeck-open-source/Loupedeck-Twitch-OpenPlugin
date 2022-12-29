namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    public class OnGiftedSubscriptionArgs : EventArgs
    {
        /// <summary>Property representing the information of the gifted subscription.</summary>
        public GiftedSubscription GiftedSubscription;
        /// <summary>Property representing the Twitch channel this event fired from.</summary>
        public String Channel;
    }
}
