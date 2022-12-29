namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    public class OnCommunitySubscriptionArgs
    {
        /// <summary>Property representing the information of the community subscription.</summary>
        public CommunitySubscription GiftedSubscription;
        /// <summary>Property representing the Twitch channel this event fired from.</summary>
        public String Channel;
    }
}
