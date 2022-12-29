namespace TwitchLib.Client.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Communication.Events;
    using Events;
    using Models;

    public interface ITwitchClient : IDisposable
    {
        Boolean AutoReListenOnException { get; set; }
        MessageEmoteCollection ChannelEmotes { get; }
        ConnectionCredentials ConnectionCredentials { get; }
        Boolean DisableAutoPong { get; set; }
        Boolean IsConnected { get; }
        Boolean IsInitialized { get; }
        IReadOnlyList<JoinedChannel> JoinedChannels { get; }
        Boolean OverrideBeingHostedCheck { get; set; }
        WhisperMessage PreviousWhisper { get; }
        String TwitchUsername { get; }
        Boolean WillReplaceEmotes { get; set; }

        event EventHandler<OnBeingHostedArgs> OnBeingHosted;
        event EventHandler<OnChannelStateChangedArgs> OnChannelStateChanged;
        event EventHandler<OnChatClearedArgs> OnChatCleared;
        event EventHandler<OnChatColorChangedArgs> OnChatColorChanged;
        event EventHandler<OnChatCommandReceivedArgs> OnChatCommandReceived;
        event EventHandler<OnConnectedArgs> OnConnected;
        event EventHandler<OnConnectionErrorArgs> OnConnectionError;
        event EventHandler<OnDisconnectedEventArgs> OnDisconnected;
        event EventHandler<OnExistingUsersDetectedArgs> OnExistingUsersDetected;
        event EventHandler<OnGiftedSubscriptionArgs> OnGiftedSubscription;
        event EventHandler<OnHostingStartedArgs> OnHostingStarted;
        event EventHandler<OnHostingStoppedArgs> OnHostingStopped;
        event EventHandler OnHostLeft;
        event EventHandler<OnIncorrectLoginArgs> OnIncorrectLogin;
        event EventHandler<OnJoinedChannelArgs> OnJoinedChannel;
        event EventHandler<OnLeftChannelArgs> OnLeftChannel;
        event EventHandler<OnLogArgs> OnLog;
        event EventHandler<OnMessageReceivedArgs> OnMessageReceived;
        event EventHandler<OnMessageSentArgs> OnMessageSent;
        event EventHandler<OnModeratorJoinedArgs> OnModeratorJoined;
        event EventHandler<OnModeratorLeftArgs> OnModeratorLeft;
        event EventHandler<OnModeratorsReceivedArgs> OnModeratorsReceived;
        event EventHandler<OnNewSubscriberArgs> OnNewSubscriber;
        event EventHandler<OnNowHostingArgs> OnNowHosting;
        event EventHandler<OnRaidNotificationArgs> OnRaidNotification;
        event EventHandler<OnReSubscriberArgs> OnReSubscriber;
        event EventHandler<OnSendReceiveDataArgs> OnSendReceiveData;
        event EventHandler<OnUserBannedArgs> OnUserBanned;
        event EventHandler<OnUserJoinedArgs> OnUserJoined;
        event EventHandler<OnUserLeftArgs> OnUserLeft;
        event EventHandler<OnUserStateChangedArgs> OnUserStateChanged;
        event EventHandler<OnUserTimedoutArgs> OnUserTimedout;
        event EventHandler<OnWhisperCommandReceivedArgs> OnWhisperCommandReceived;
        event EventHandler<OnWhisperReceivedArgs> OnWhisperReceived;
        event EventHandler<OnWhisperSentArgs> OnWhisperSent;
        event EventHandler<OnMessageThrottledEventArgs> OnMessageThrottled;
        event EventHandler<OnWhisperThrottledEventArgs> OnWhisperThrottled;
        event EventHandler<OnErrorEventArgs> OnError;
        event EventHandler<OnReconnectedEventArgs> OnReconnected;

        void Initialize(ConnectionCredentials credentials, String channel = null, Char chatCommandIdentifier = '!', Char whisperCommandIdentifier = '!', Boolean autoReListenOnExceptions = true);

        void SetConnectionCredentials(ConnectionCredentials credentials);

        void AddChatCommandIdentifier(Char identifier);
        void AddWhisperCommandIdentifier(Char identifier);
        void RemoveChatCommandIdentifier(Char identifier);
        void RemoveWhisperCommandIdentifier(Char identifier);

        void Connect();
        void Disconnect();
        void Reconnect();

        JoinedChannel GetJoinedChannel(String channel);

        void JoinChannel(String channel, Boolean overrideCheck = false);
        void JoinRoom(String channelId, String roomId, Boolean overrideCheck = false);
        void LeaveChannel(JoinedChannel channel);
        void LeaveChannel(String channel);
        void LeaveRoom(String channelId, String roomId);

        void OnReadLineTest(String rawIrc);

        void SendMessage(JoinedChannel channel, String message, Boolean dryRun = false);
        void SendMessage(String channel, String message, Boolean dryRun = false);
        void SendQueuedItem(String message);
        void SendRaw(String message);
        void SendWhisper(String receiver, String message, Boolean dryRun = false);
    }
}