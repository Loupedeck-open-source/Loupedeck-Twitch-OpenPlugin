namespace TwitchLib.Client.Models
{
    using System;
    using System.Collections.Generic;
    using Enums;
    using Internal;

    public class ReSubscriber : SubscriberBase
    {
        public Int32 Months { get; }

        public ReSubscriber(IrcMessage ircMessage) : base(ircMessage) {
            this.Months = this.months;
        }
        public ReSubscriber(List<KeyValuePair<String, String>> badges, String colorHex, String displayName, String emoteSet, String id, String login, String systemMessage,
            String systemMessageParsed, String resubMessage, SubscriptionPlan subscriptionPlan, String subscriptionPlanName, String roomId, String userId, Boolean isModerator, Boolean isTurbo,
            Boolean isSubscriber, Boolean isPartner, String tmiSentTs, UserType userType, String rawIrc, String channel) : base(badges, colorHex, displayName, emoteSet, id, login, systemMessage,
                systemMessageParsed, resubMessage, subscriptionPlan, subscriptionPlanName, roomId, userId, isModerator, isTurbo, isSubscriber, isPartner, tmiSentTs, userType, rawIrc, channel) { }
    }
}
