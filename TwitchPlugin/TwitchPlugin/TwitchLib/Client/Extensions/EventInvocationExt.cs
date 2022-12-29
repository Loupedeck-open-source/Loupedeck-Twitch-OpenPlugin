namespace TwitchLib.Client.Extensions
{
    using System;
    using System.Collections.Generic;
    using Enums;
    using Events;
    using Models;

    public static class EventInvocationExt
    {
        public static void InvokeOnBeingHosted(this TwitchClient client, String channel, String botUsername, String hostedByChannel, Int32 viewers, Boolean isAutoHosted)
        {
            var model = new OnBeingHostedArgs()
            {
                BeingHostedNotification = new BeingHostedNotification(channel, botUsername, hostedByChannel, viewers, isAutoHosted)
            };
            client.RaiseEvent("OnBeingHosted", model);
        }

        public static void InvokeChannelStateChanged(this TwitchClient client, String channel, Boolean r9k, Boolean rituals,
            Boolean subOnly, Int32 slowMode, Boolean emoteOnly, String broadcasterLanguage, TimeSpan followersOnly, Boolean mercury, String roomId)
        {
            var state = new ChannelState(r9k, rituals, subOnly, slowMode, emoteOnly, broadcasterLanguage, channel, followersOnly, mercury, roomId);
            var model = new OnChannelStateChangedArgs()
            {
                Channel = channel,
                ChannelState = state
            };
            client.RaiseEvent("OnChannelStateChanged", model);
        }

        public static void InvokeChatCleared(this TwitchClient client, String channel)
        {
            var model = new OnChatClearedArgs()
            {
                Channel = channel
            };
            client.RaiseEvent("OnChatCleared", model);
        }

        public static void InvokeChatCommandsReceived(this TwitchClient client, String botUsername, String userId, String userName, String displayName,
            String colorHex, EmoteSet emoteSet, String message, UserType userType, String channel, String id, Boolean isSubscriber, Int32 subscribedMonthCount,
            String roomId, Boolean isTurbo, Boolean isModerator, Boolean isMe, Boolean isBroadcaster, Noisy noisy, String rawIrcMessage, String emoteReplacedMessage,
            List<KeyValuePair<String, String>> badges, CheerBadge cheerBadge, Int32 bits, Double bitsInDollars, String commandText, String argumentsAsString,
            List<String> argumentsAsList, Char commandIdentifier)
        {
            var msg = new ChatMessage(botUsername, userId, userName, displayName, colorHex, emoteSet, message, userType, channel, id,
                isSubscriber, subscribedMonthCount, roomId, isTurbo, isModerator, isMe, isBroadcaster, noisy, rawIrcMessage, emoteReplacedMessage,
                badges, cheerBadge, bits, bitsInDollars);
            var model = new OnChatCommandReceivedArgs()
            {
                Command = new ChatCommand(msg, commandText, argumentsAsString, argumentsAsList, commandIdentifier)
            };
            client.RaiseEvent("OnChatCommandReceived", model);
        }

        public static void InvokeConnected(this TwitchClient client, String autoJoinChannel, String botUsername)
        {
            var model = new OnConnectedArgs()
            {
                AutoJoinChannel = autoJoinChannel,
                BotUsername = botUsername
            };
            client.RaiseEvent("OnConnected", model);
        }

        public static void InvokeConnectionError(this TwitchClient client, String botUsername, ErrorEvent errorEvent)
        {
            var model = new OnConnectionErrorArgs()
            {
                BotUsername = botUsername,
                Error = errorEvent
            };
            client.RaiseEvent("OnConnectionError", model);
        }

        public static void InvokeDisconnected(this TwitchClient client, String botUsername)
        {
            var model = new OnDisconnectedArgs()
            {
                BotUsername = botUsername
            };
            client.RaiseEvent("OnDisconnected", model);
        }

        public static void InvokeExistingUsersDetected(this TwitchClient client, String channel, List<String> users)
        {
            var model = new OnExistingUsersDetectedArgs()
            {
                Channel = channel,
                Users = users
            };
            client.RaiseEvent("OnExistingUsersDetected", model);
        }

        public static void InvokeGiftedSubscription(this TwitchClient client, String badges, String color, String displayName, String emotes, String id, String login, Boolean isModerator,
            String msgId, String msgParamMonths, String msgParamRecipientDisplayName, String msgParamRecipientId, String msgParamRecipientUserName,
            String msgParamSubPlanName, SubscriptionPlan msgParamSubPlan, String roomId, Boolean isSubscriber, String systemMsg, String systemMsgParsed,
            String tmiSentTs, Boolean isTurbo, UserType userType)
        {
            var model = new OnGiftedSubscriptionArgs()
            {
                GiftedSubscription = new GiftedSubscription(badges, color, displayName, emotes, id, login, isModerator, msgId, msgParamMonths, msgParamRecipientDisplayName,
                msgParamRecipientId, msgParamRecipientUserName, msgParamSubPlanName, msgParamSubPlan, roomId, isSubscriber, systemMsg, systemMsgParsed, tmiSentTs, isTurbo,
                userType)
            };
            client.RaiseEvent("OnGiftedSubscription", model);
        }

        public static void InvokeOnHostingStarted(this TwitchClient client, String hostingChannel, String targetChannel, Int32 viewers)
        {
            var model = new OnHostingStartedArgs()
            {
                HostingStarted = new HostingStarted(hostingChannel, targetChannel, viewers)
            };
            client.RaiseEvent("OnHostingStarted", model);
        }

        public static void InvokeOnHostingStopped(this TwitchClient client, String hostingChannel, Int32 viewers)
        {
            var model = new OnHostingStoppedArgs()
            {
                HostingStopped = new HostingStopped(hostingChannel, viewers)
            };
            client.RaiseEvent("OnHostingStopped", model);
        }

        public static void InvokeHostLeft(this TwitchClient client)
        {
            client.RaiseEvent("OnHostLeft");
        }

        public static void InvokeIncorrectLogin(this TwitchClient client, Exceptions.ErrorLoggingInException ex)
        {
            var model = new OnIncorrectLoginArgs()
            {
                Exception = ex
            };
            client.RaiseEvent("OnIncorrectLogin", model);
        }

        public static void InvokeJoinedChannel(this TwitchClient client, String botUsername, String channel)
        {
            var model = new OnJoinedChannelArgs()
            {
                BotUsername = botUsername,
                Channel = channel
            };
            client.RaiseEvent("OnJoinedChannel", model);
        }

        public static void InvokeLeftChannel(this TwitchClient client, String botUsername, String channel)
        {
            var model = new OnLeftChannelArgs()
            {
                BotUsername = botUsername,
                Channel = channel
            };
            client.RaiseEvent("OnLeftChannel", model);
        }

        public static void InvokeLog(this TwitchClient client, String botUsername, String data, DateTime dateTime)
        {
            var model = new OnLogArgs()
            {
                BotUsername = botUsername,
                Data = data,
                DateTime = dateTime
            };
            client.RaiseEvent("OnLog", model);
        }

        public static void InvokeMessageReceived(this TwitchClient client, String botUsername, String userId, String userName, String displayName, String colorHex,
            EmoteSet emoteSet, String message, UserType userType, String channel, String id, Boolean isSubscriber, Int32 subscribedMonthCount, String roomId, Boolean isTurbo,
            Boolean isModerator, Boolean isMe, Boolean isBroadcaster, Noisy noisy, String rawIrcMessage, String emoteReplacedMessage, List<KeyValuePair<String, String>> badges,
            CheerBadge cheerBadge, Int32 bits, Double bitsInDollars)
        {
            var model = new OnMessageReceivedArgs()
            {
                ChatMessage = new ChatMessage(botUsername, userId, userName, displayName, colorHex, emoteSet, message, userType, channel, id, isSubscriber,
                subscribedMonthCount, roomId, isTurbo, isModerator, isMe, isBroadcaster, noisy, rawIrcMessage, emoteReplacedMessage, badges, cheerBadge, bits,
                bitsInDollars)
            };
            client.RaiseEvent("OnMessageReceived", model);
        }

        public static void InvokeMessageSent(this TwitchClient client, List<KeyValuePair<String, String>> badges, String channel, String colorHex,
            String displayName, String emoteSet, Boolean isModerator, Boolean isSubscriber, UserType userType, String message)
        {
            var model = new OnMessageSentArgs()
            {
                SentMessage = new SentMessage(badges, channel, colorHex, displayName, emoteSet, isModerator, isSubscriber, userType, message)
            };
            client.RaiseEvent("OnMessageSent", model);
        }

        public static void InvokeModeratorJoined(this TwitchClient client, String channel, String username)
        {
            var model = new OnModeratorJoinedArgs()
            {
                Channel = channel,
                Username = username
            };
            client.RaiseEvent("OnModeratorJoined", model);
        }

        public static void InvokeModeratorLeft(this TwitchClient client, String channel, String username)
        {
            var model = new OnModeratorLeftArgs()
            {
                Channel = channel,
                Username = username
            };
            client.RaiseEvent("OnModeratorLeft", model);
        }

        public static void InvokeModeratorsReceived(this TwitchClient client, String channel, List<String> moderators)
        {
            var model = new OnModeratorsReceivedArgs()
            {
                Channel = channel,
                Moderators = moderators
            };
            client.RaiseEvent("OnModeratorsReceived", model);
        }

        public static void InvokeNewSubscriber(this TwitchClient client, List<KeyValuePair<String, String>> badges, String colorHex, String displayName,
            String emoteSet, String id, String login, String systemMessage, String systemMessageParsed, String resubMessage, SubscriptionPlan subscriptionPlan,
            String subscriptionPlanName, String roomId, String userId, Boolean isModerator, Boolean isTurbo, Boolean isSubscriber, Boolean isPartner, String tmiSentTs,
            UserType userType, String rawIrc, String channel)
        {
            var model = new OnNewSubscriberArgs()
            {
                Subscriber = new Subscriber(badges, colorHex, displayName, emoteSet, id, login, systemMessage, systemMessageParsed, resubMessage,
                subscriptionPlan, subscriptionPlanName, roomId, userId, isModerator, isTurbo, isSubscriber, isPartner, tmiSentTs, userType, rawIrc, channel)
            };
            client.RaiseEvent("OnNewSubscriber", model);
        }

        public static void InvokeNowHosting(this TwitchClient client, String channel, String hostedChannel)
        {
            var model = new OnNowHostingArgs()
            {
                Channel = channel,
                HostedChannel = hostedChannel
            };
            client.RaiseEvent("OnNowHosting", model);
        }

        public static void InvokeRaidNotification(this TwitchClient client, String channel, String badges, String color, String displayName, String emotes, String id, String login, Boolean moderator, String msgId, String msgParamDisplayName,
            String msgParamLogin, String msgParamViewerCount, String roomId, Boolean subscriber, String systemMsg, String systemMsgParsed, String tmiSentTs, Boolean turbo, UserType userType)
        {
            var model = new OnRaidNotificationArgs()
            {
                Channel = channel,
                RaidNotificaiton = new RaidNotification(badges, color, displayName, emotes, id, login, moderator, msgId, msgParamDisplayName, msgParamLogin, msgParamViewerCount,
                roomId, subscriber, systemMsg, systemMsgParsed, tmiSentTs, turbo, userType)
            };
            client.RaiseEvent("OnRaidNotification", model);
        }

        public static void InvokeReSubscriber(this TwitchClient client, List<KeyValuePair<String, String>> badges, String colorHex, String displayName,
            String emoteSet, String id, String login, String systemMessage, String systemMessageParsed, String resubMessage, SubscriptionPlan subscriptionPlan,
            String subscriptionPlanName, String roomId, String userId, Boolean isModerator, Boolean isTurbo, Boolean isSubscriber, Boolean isPartner, String tmiSentTs,
            UserType userType, String rawIrc, String channel)
        {
            var model = new OnReSubscriberArgs()
            {
                ReSubscriber = new ReSubscriber(badges, colorHex, displayName, emoteSet, id, login, systemMessage, systemMessageParsed, resubMessage,
                subscriptionPlan, subscriptionPlanName, roomId, userId, isModerator, isTurbo, isSubscriber, isPartner, tmiSentTs, userType, rawIrc, channel)
            };
            client.RaiseEvent("OnReSubscriber", model);
        }

        public static void InvokeSendReceiveData(this TwitchClient client, String data, SendReceiveDirection direction)
        {
            var model = new OnSendReceiveDataArgs()
            {
                Data = data,
                Direction = direction
            };
            client.RaiseEvent("OnSendReceiveData", model);
        }

        public static void InvokeUserBanned(this TwitchClient client, String channel, String username, String banReason)
        {
            var model = new OnUserBannedArgs()
            {
                UserBan = new UserBan(channel, username, banReason)
            };
            client.RaiseEvent("OnUserBanned", model);
        }

        public static void InvokeUserJoined(this TwitchClient client, String channel, String username)
        {
            var model = new OnUserJoinedArgs()
            {
                Channel = channel,
                Username = username
            };
            client.RaiseEvent("OnUserJoined", model);
        }

        public static void InvokeUserLeft(this TwitchClient client, String channel, String username)
        {
            var model = new OnUserLeftArgs()
            {
                Channel = channel,
                Username = username
            };
            client.RaiseEvent("OnUserLeft", model);
        }

        public static void InvokeUserStateChanged(this TwitchClient client, List<KeyValuePair<String, String>> badges, String colorHex, String displayName,
            String emoteSet, String channel, Boolean isSubscriber, Boolean isModerator, UserType userType)
        {
            var model = new OnUserStateChangedArgs()
            {
                UserState = new UserState(badges, colorHex, displayName, emoteSet, channel, isSubscriber, isModerator, userType)
            };
            client.RaiseEvent("OnUserStateChanged", model);
        }

        public static void InvokeUserTimedout(this TwitchClient client, String channel, String username, Int32 timeoutDuration, String timeoutReason)
        {
            var model = new OnUserTimedoutArgs()
            {
                UserTimeout = new UserTimeout(channel, username, timeoutDuration, timeoutReason)
            };
            client.RaiseEvent("OnUserTimedout", model);
        }

        public static void InvokeWhisperCommandReceived(this TwitchClient client, List<KeyValuePair<String, String>> badges, String colorHex, String username, String displayName, EmoteSet emoteSet, String threadId, String messageId,
            String userId, Boolean isTurbo, String botUsername, String message, UserType userType, String commandText, String argumentsAsString, List<String> argumentsAsList, Char commandIdentifier)
        {
            var whisperMsg = new WhisperMessage(badges, colorHex, username, displayName, emoteSet, threadId, messageId, userId, isTurbo, botUsername, message, userType);
            var model = new OnWhisperCommandReceivedArgs()
            {
                Command = new WhisperCommand(whisperMsg, commandText, argumentsAsString, argumentsAsList, commandIdentifier)
            };
            client.RaiseEvent("OnWhisperCommandReceived", model);
        }

        public static void InvokeWhisperReceived(this TwitchClient client, List<KeyValuePair<String, String>> badges, String colorHex, String username, String displayName, EmoteSet emoteSet, String threadId, String messageId,
            String userId, Boolean isTurbo, String botUsername, String message, UserType userType)
        {
            var model = new OnWhisperReceivedArgs()
            {
                WhisperMessage = new WhisperMessage(badges, colorHex, username, displayName, emoteSet, threadId, messageId, userId, isTurbo, botUsername, message, userType)
            };
            client.RaiseEvent("OnWhisperReceived", model);
        }

        public static void InvokeWhisperSent(this TwitchClient client, String username, String receiver, String message)
        {
            var model = new OnWhisperSentArgs()
            {
                Message = message,
                Receiver = receiver,
                Username = username
            };
            client.RaiseEvent("OnWhisperSent", model);
        }
    }
}
