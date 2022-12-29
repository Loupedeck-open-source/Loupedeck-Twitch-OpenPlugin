namespace TwitchLib.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Communication.Clients;
    using Communication.Events;
    using Communication.Interfaces;
    using Enums;
    using Enums.Internal;
    using Events;
    using Exceptions;
    using Interfaces;
    using Internal;
    using Internal.Parsing;
    using Loupedeck.TwitchPlugin;
    using Manager;
    using Models;
    using Models.Internal;

    /// <summary>Represents a client connected to a Twitch channel.</summary>
    public class TwitchClient : ITwitchClient 
    {
        #region Private Variables
        private IClient _client;
        private MessageEmoteCollection _channelEmotes = new MessageEmoteCollection();
        private readonly ICollection<Char> _chatCommandIdentifiers = new HashSet<Char>();
        private readonly ICollection<Char> _whisperCommandIdentifiers = new HashSet<Char>();
        private readonly Queue<JoinedChannel> _joinChannelQueue = new Queue<JoinedChannel>();
        private readonly ILogger _logger;
        private readonly ClientProtocol _protocol;
        private String _autoJoinChannel;
        private Boolean _currentlyJoiningChannels;
        private System.Timers.Timer _joinTimer;
        private List<KeyValuePair<String, DateTime>> _awaitingJoins;
        private ChannelState _channelState;

        private readonly IrcParser _ircParser;
        private readonly JoinedChannelManager _joinedChannelManager;

        // variables used for constructing OnMessageSent properties
        private readonly List<String> _hasSeenJoinedChannels = new List<String>();
        private String _lastMessageSent;
        #endregion

        #region Public Variables
        /// <summary>Assembly version of TwitchLib.Client.</summary>
        public Version Version => Assembly.GetEntryAssembly().GetName().Version;
        /// <summary>Checks if underlying client has been initialized.</summary>
        public Boolean IsInitialized => this._client != null;
        /// <summary>A list of all channels the client is currently in.</summary>
        public IReadOnlyList<JoinedChannel> JoinedChannels => this._joinedChannelManager.GetJoinedChannels();
        /// <summary>Username of the user connected via this library.</summary>
        public String TwitchUsername { get; private set; }
        /// <summary>The most recent whisper received.</summary>
        public WhisperMessage PreviousWhisper { get; private set; }
        /// <summary>The current connection status of the client.</summary>
        public Boolean IsConnected => this.IsInitialized && this._client != null ? this._client.IsConnected : false;
       
        /// <summary>The emotes this channel replaces.</summary>
        /// <remarks>
        ///     Twitch-handled emotes are automatically added to this collection (which also accounts for
        ///     managing user emote permissions such as sub-only emotes). Third-party emotes will have to be manually
        ///     added according to the availability rules defined by the third-party.
        /// </remarks>
        public MessageEmoteCollection ChannelEmotes => this._channelEmotes;

        /// <summary>Will disable the client from sending automatic PONG responses to PING</summary>
        public Boolean DisableAutoPong { get; set; } = false;
        /// <summary>Determines whether Emotes will be replaced in messages.</summary>
        public Boolean WillReplaceEmotes { get; set; } = false;
        /// <summary>If set to true, the library will not check upon channel join that if BeingHosted event is subscribed, that the bot is connected as broadcaster. Only override if the broadcaster is joining multiple channels, including the broadcaster's.</summary>
        public Boolean OverrideBeingHostedCheck { get; set; } = false;
        /// <summary>Provides access to connection credentials object.</summary>
        public ConnectionCredentials ConnectionCredentials { get; private set; }
        /// <summary>Provides access to autorelistiononexception on off boolean.</summary>
        public Boolean AutoReListenOnException { get; set; }

        #endregion

        #region Events
        /// <summary>
        /// Fires whenever a log write happens.
        /// </summary>
        public event EventHandler<OnLogArgs> OnLog;

        /// <summary>
        /// Fires when client connects to Twitch.
        /// </summary>
        public event EventHandler<OnConnectedArgs> OnConnected;

        /// <summary>
        /// Fires when client joins a channel.
        /// </summary>
        public event EventHandler<OnJoinedChannelArgs> OnJoinedChannel;

        /// <summary>
        /// Fires on logging in with incorrect details, returns ErrorLoggingInException.
        /// </summary>
        public event EventHandler<OnIncorrectLoginArgs> OnIncorrectLogin;

        /// <summary>
        /// Fires when connecting and channel state is changed, returns ChannelState.
        /// </summary>
        public event EventHandler<OnChannelStateChangedArgs> OnChannelStateChanged;

        /// <summary>
        /// Fires when a user state is received, returns UserState.
        /// </summary>
        public event EventHandler<OnUserStateChangedArgs> OnUserStateChanged;

        /// <summary>
        /// Fires when a new chat message arrives, returns ChatMessage.
        /// </summary>
        public event EventHandler<OnMessageReceivedArgs> OnMessageReceived;

        /// <summary>
        /// Fires when a new whisper arrives, returns WhisperMessage.
        /// </summary>
        public event EventHandler<OnWhisperReceivedArgs> OnWhisperReceived;

        /// <summary>
        /// Fires when a chat message is sent, returns username, channel and message.
        /// </summary>
        public event EventHandler<OnMessageSentArgs> OnMessageSent;

        /// <summary>
        /// Fires when a whisper message is sent, returns username and message.
        /// </summary>
        public event EventHandler<OnWhisperSentArgs> OnWhisperSent;

        /// <summary>
        /// Fires when command (uses custom chat command identifier) is received, returns channel, command, ChatMessage, arguments as string, arguments as list.
        /// </summary>
        public event EventHandler<OnChatCommandReceivedArgs> OnChatCommandReceived;

        /// <summary>
        /// Fires when command (uses custom whisper command identifier) is received, returns command, Whispermessage.
        /// </summary>
        public event EventHandler<OnWhisperCommandReceivedArgs> OnWhisperCommandReceived;

        /// <summary>
        /// Fires when a new viewer/chatter joined the channel's chat room, returns username and channel.
        /// </summary>
        public event EventHandler<OnUserJoinedArgs> OnUserJoined;

        /// <summary>
        /// Fires when a moderator joined the channel's chat room, returns username and channel.
        /// </summary>
        public event EventHandler<OnModeratorJoinedArgs> OnModeratorJoined;

        /// <summary>
        /// Fires when a moderator joins the channel's chat room, returns username and channel.
        /// </summary>
        public event EventHandler<OnModeratorLeftArgs> OnModeratorLeft;

        /// <summary>
        /// Fires when new subscriber is announced in chat, returns Subscriber.
        /// </summary>
        public event EventHandler<OnNewSubscriberArgs> OnNewSubscriber;

        /// <summary>
        /// Fires when current subscriber renews subscription, returns ReSubscriber.
        /// </summary>
        public event EventHandler<OnReSubscriberArgs> OnReSubscriber;

        /// <summary>
        /// Fires when a hosted streamer goes offline and hosting is killed.
        /// </summary>
        public event EventHandler OnHostLeft;

        /// <summary>
        /// Fires when Twitch notifies client of existing users in chat.
        /// </summary>
        public event EventHandler<OnExistingUsersDetectedArgs> OnExistingUsersDetected;

        /// <summary>
        /// Fires when a PART message is received from Twitch regarding a particular viewer
        /// </summary>
        public event EventHandler<OnUserLeftArgs> OnUserLeft;

        /// <summary>
        /// Fires when the joined channel begins hosting another channel.
        /// </summary>
        public event EventHandler<OnHostingStartedArgs> OnHostingStarted;

        /// <summary>
        /// Fires when the joined channel quits hosting another channel.
        /// </summary>
        public event EventHandler<OnHostingStoppedArgs> OnHostingStopped;

        /// <summary>
        /// Fires when bot has disconnected.
        /// </summary>
        public event EventHandler<OnDisconnectedEventArgs> OnDisconnected;

        /// <summary>
        /// Forces when bot suffers conneciton error.
        /// </summary>
        public event EventHandler<OnConnectionErrorArgs> OnConnectionError;

        /// <summary>
        /// Fires when a channel's chat is cleared.
        /// </summary>
        public event EventHandler<OnChatClearedArgs> OnChatCleared;

        /// <summary>
        /// Fires when a viewer gets timedout by any moderator.
        /// </summary>
        public event EventHandler<OnUserTimedoutArgs> OnUserTimedout;

        /// <summary>
        /// Fires when client successfully leaves a channel.
        /// </summary>
        public event EventHandler<OnLeftChannelArgs> OnLeftChannel;

        /// <summary>
        /// Fires when a viewer gets banned by any moderator.
        /// </summary>
        public event EventHandler<OnUserBannedArgs> OnUserBanned;

        /// <summary>
        /// Fires when a list of moderators is received.
        /// </summary>
        public event EventHandler<OnModeratorsReceivedArgs> OnModeratorsReceived;

        /// <summary>
        /// Fires when confirmation of a chat color change request was received.
        /// </summary>
        public event EventHandler<OnChatColorChangedArgs> OnChatColorChanged;

        /// <summary>
        /// Fires when data is either received or sent.
        /// </summary>
        public event EventHandler<OnSendReceiveDataArgs> OnSendReceiveData;

        /// <summary>
        /// Fires when client receives notice that a joined channel is hosting another channel.
        /// </summary>
        public event EventHandler<OnNowHostingArgs> OnNowHosting;

        /// <summary>
        /// Fires when the library detects another channel has started hosting the broadcaster's stream. MUST BE CONNECTED AS BROADCASTER.
        /// </summary>
        public event EventHandler<OnBeingHostedArgs> OnBeingHosted;

        /// <summary>
        /// Fires when a raid notification is detected in chat
        /// </summary>
        public event EventHandler<OnRaidNotificationArgs> OnRaidNotification;

        /// <summary>
        /// Fires when a subscription is gifted and announced in chat
        /// </summary>
        public event EventHandler<OnGiftedSubscriptionArgs> OnGiftedSubscription;

        /// <summary>
        /// Fires when a community subscription is announced in chat
        /// </summary>
        public event EventHandler<OnCommunitySubscriptionArgs> OnCommunitySubscription;

        /// <summary>
        /// Fires when a Message has been throttled.
        /// </summary>
        public event EventHandler<OnMessageThrottledEventArgs> OnMessageThrottled;

        /// <summary>
        /// Fires when a Whisper has been throttled.
        /// </summary>
        public event EventHandler<OnWhisperThrottledEventArgs> OnWhisperThrottled;

        /// <summary>
        /// Occurs when an Error is thrown in the protocol client
        /// </summary>
        public event EventHandler<OnErrorEventArgs> OnError;

        /// <summary>
        /// Occurs when a reconnection occurs.
        /// </summary>
        public event EventHandler<OnReconnectedEventArgs> OnReconnected;

        /// <summary>Fires when TwitchClient attempts to host a channel it is in.</summary>
        public EventHandler OnSelfRaidError;

        /// <summary>Fires when TwitchClient receives generic no permission error from Twitch.</summary>
        public EventHandler OnNoPermissionError;

        /// <summary>Fires when newly raided channel is mature audience only.</summary>
        public EventHandler OnRaidedChannelIsMatureAudience;

        /// <summary>Fires when a ritual for a new chatter is received.</summary>
        public EventHandler<OnRitualNewChatterArgs> OnRitualNewChatter;

        /// <summary>Fires when the client was unable to join a channel.</summary>
        public EventHandler<OnFailureToReceiveJoinConfirmationArgs> OnFailureToReceiveJoinConfirmation;

        /// <summary>Fires when data is received from Twitch that is not able to be parsed.</summary>
        public EventHandler<OnUnaccountedForArgs> OnUnaccountedFor;
        #endregion

        #region Construction Work

        /// <summary>
        /// Initializes the TwitchChatClient class.
        /// </summary>
        /// <param name="client">Protocol Client to use for connection from TwitchLib.Communication. Possible Options Are the TcpClient client or WebSocket client.</param>
        /// <param name="logger">Optional ILogger instance to enable logging</param>
        public TwitchClient(IClient client = null, ClientProtocol protocol = ClientProtocol.WebSocket, ILogger logger = null)
        {
            this._logger = logger;
            this._client = client;
            this._protocol = protocol;
            this._joinedChannelManager = new JoinedChannelManager();
            this._ircParser = new IrcParser();
            this._channelState = new ChannelState();
        }

        /// <summary>
        /// Initializes the TwitchChatClient class.
        /// </summary>
        /// <param name="channel">The channel to connect to.</param>
        /// <param name="credentials">The credentials to use to log in.</param>
        /// <param name="chatCommandIdentifier">The identifier to be used for reading and writing commands from chat.</param>
        /// <param name="whisperCommandIdentifier">The identifier to be used for reading and writing commands from whispers.</param>
        /// <param name="autoReListenOnExceptions">By default, TwitchClient will silence exceptions and auto-relisten for overall stability. For debugging, you may wish to have the exception bubble up, set this to false.</param>
        public void Initialize(ConnectionCredentials credentials, String channel = null, Char chatCommandIdentifier = '!', Char whisperCommandIdentifier = '!', Boolean autoReListenOnExceptions = true)
        {
            this.Log($"TwitchLib-TwitchClient initialized, assembly version: {Assembly.GetExecutingAssembly().GetName().Version}");
            this.ConnectionCredentials = credentials;
            this.TwitchUsername = this.ConnectionCredentials.TwitchUsername;
            this._autoJoinChannel = channel?.ToLower();
            if (chatCommandIdentifier != '\0')
                this._chatCommandIdentifiers.Add(chatCommandIdentifier);
            if (whisperCommandIdentifier != '\0')
                this._whisperCommandIdentifiers.Add(whisperCommandIdentifier);

            this.AutoReListenOnException = autoReListenOnExceptions;

            this.InitializeClient();
        }

        private void InitializeClient()
        {
            if (this._client == null)
            {
                switch (this._protocol)
                {
                    case ClientProtocol.TCP:
                        this._client = new TcpClient();
                        break;
                    case ClientProtocol.WebSocket:
                        this._client = new WebSocketClient();
                        break;
                }
            }

            Debug.Assert(this._client != null, nameof(this._client) + " != null");

            this._client.OnConnected += this._client_OnConnected;
            this._client.OnMessage += this._client_OnMessage;
            this._client.OnDisconnected += this._client_OnDisconnected;
            this._client.OnFatality += this._client_OnFatality;
            this._client.OnMessageThrottled += this._client_OnMessageThrottled;
            this._client.OnWhisperThrottled += this._client_OnWhisperThrottled;
            this._client.OnReconnected += this._client_OnReconnected;
            this._client.OnError += this._client_OnError;
        }

        #endregion

        internal void RaiseEvent(String eventName, Object args = null)
        {
            var fInfo = this.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic) as FieldInfo;
            var multi = fInfo.GetValue(this) as MulticastDelegate;
            foreach (Delegate del in multi.GetInvocationList())
            {
                del.Method.Invoke(del.Target, args == null ? new Object[] {this, new EventArgs()} : new[] {this, args});
            }
        }

        /// <summary>
        /// Sends a RAW IRC message.
        /// </summary>
        /// <param name="message">The RAW message to be sent.</param>
        public void SendRaw(String message)
        {
            if (!this.IsInitialized) HandleNotInitialized();

            this.Log($"Writing: {message}");
            this._client.Send(message);
            this.OnSendReceiveData?.Invoke(this, new OnSendReceiveDataArgs { Direction = SendReceiveDirection.Sent, Data = message });
        }

        #region SendMessage

        /// <summary>
        /// Sends a formatted Twitch channel chat message.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="dryRun">If set to true, the message will not actually be sent for testing purposes.</param>
        /// <param name="channel">Channel to send message to.</param>
        public void SendMessage(JoinedChannel channel, String message, Boolean dryRun = false)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            if (channel == null || message == null || dryRun) return;
            if (message.Length > 500)
            {
                this.LogError("Message length has exceeded the maximum character count. (500)");
                return;
            }

            var twitchMessage = new OutboundChatMessage
            {
                Channel = channel.Channel,
                Username = this.ConnectionCredentials.TwitchUsername,
                Message = message
            };

            this._lastMessageSent = message;


            this._client.Send(twitchMessage.ToString());
        }

        /// <summary>
        /// SendMessage wrapper that accepts channel in string form.
        /// </summary>
        public void SendMessage(String channel, String message, Boolean dryRun = false)
        {
            this.SendMessage(this.GetJoinedChannel(channel), message, dryRun);
        }

        #endregion

        #region Whispers
        /// <summary>
        /// Sends a formatted whisper message to someone.
        /// </summary>
        /// <param name="receiver">The receiver of the whisper.</param>
        /// <param name="message">The message to be sent.</param>
        /// <param name="dryRun">If set to true, the message will not actually be sent for testing purposes.</param>
        public void SendWhisper(String receiver, String message, Boolean dryRun = false)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            if (dryRun) return;

            var twitchMessage = new OutboundWhisperMessage
            {
                Receiver = receiver,
                Username = this.ConnectionCredentials.TwitchUsername,
                Message = message
            };
            
            this._client.SendWhisper(twitchMessage.ToString());

            this.OnWhisperSent?.Invoke(this, new OnWhisperSentArgs { Receiver = receiver, Message = message });
        }

        #endregion

        #region Connection Calls
        /// <summary>
        /// Start connecting to the Twitch IRC chat.
        /// </summary>
        public void Connect()
        {
            if (!this.IsInitialized) HandleNotInitialized();
            this.Log($"Connecting to: {this.ConnectionCredentials.TwitchWebsocketURI}");

            this._client.Open();

            this.Log("Should be connected!");
        }

        /// <summary>
        /// Start disconnecting from the Twitch IRC chat.
        /// </summary>
        public void Disconnect()
        {
            this.Log("Disconnect Twitch Chat Client...");

            if (!this.IsInitialized) HandleNotInitialized();
            this._client.Close();

            // Clear instance data
            this._joinedChannelManager.Clear();
            this.PreviousWhisper = null;
        }

        /// <summary>
        /// Start reconnecting to the Twitch IRC chat.
        /// </summary>
        public void Reconnect()
        {
            if (!this.IsInitialized) HandleNotInitialized();
            this.Log($"Reconnecting to Twitch");
            this._joinedChannelManager.Clear();
            this._client.Reconnect();
        }
        #endregion

        #region Command Identifiers
        /// <summary>
        /// Adds a character to a list of characters that if found at the start of a message, fires command received event.
        /// </summary>
        /// <param name="identifier">Character, that if found at start of message, fires command received event.</param>
        public void AddChatCommandIdentifier(Char identifier)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            this._chatCommandIdentifiers.Add(identifier);
        }

        /// <summary>
        /// Removes a character from a list of characters that if found at the start of a message, fires command received event.
        /// </summary>
        /// <param name="identifier">Command identifier to removed from identifier list.</param>
        public void RemoveChatCommandIdentifier(Char identifier)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            this._chatCommandIdentifiers.Remove(identifier);
        }

        /// <summary>
        /// Adds a character to a list of characters that if found at the start of a whisper, fires command received event.
        /// </summary>
        /// <param name="identifier">Character, that if found at start of message, fires command received event.</param>
        public void AddWhisperCommandIdentifier(Char identifier)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            this._whisperCommandIdentifiers.Add(identifier);
        }

        /// <summary>
        /// Removes a character to a list of characters that if found at the start of a whisper, fires command received event.
        /// </summary>
        /// <param name="identifier">Command identifier to removed from identifier list.</param>
        public void RemoveWhisperCommandIdentifier(Char identifier)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            this._whisperCommandIdentifiers.Remove(identifier);
        }
        #endregion

        #region ConnectionCredentials

        public void SetConnectionCredentials(ConnectionCredentials credentials)
        {
            if (!this.IsInitialized)
                HandleNotInitialized();
            if (this.IsConnected)
                throw new IllegalAssignmentException("While the client is connected, you are unable to change the connection credentials. Please disconnect first and then change them.");

            this.ConnectionCredentials = credentials;
        }

        #endregion

        #region Channel Calls
        /// <summary>
        /// Join the Twitch IRC chat of <paramref name="channel"/>.
        /// </summary>
        /// <param name="channel">The channel to join.</param>
        /// <param name="overrideCheck">Override a join check.</param>
        public void JoinChannel(String channel, Boolean overrideCheck = false)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            if (!this.IsConnected) HandleNotConnected();
            // Check to see if client is already in channel
            if (this.JoinedChannels.FirstOrDefault(x => x.Channel.ToLower() == channel && !overrideCheck) != null)
                return;
            this._joinChannelQueue.Enqueue(new JoinedChannel(channel));
            if (!this._currentlyJoiningChannels)
                this.QueueingJoinCheck();
        }

        public void JoinRoom(String channelId, String roomId, Boolean overrideCheck = false)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            // Check to see if client is already in channel
            if (this.JoinedChannels.FirstOrDefault(x => x.Channel.ToLower() == $"chatrooms:{channelId}:{roomId}" && !overrideCheck) != null)
                return;
            this._joinChannelQueue.Enqueue(new JoinedChannel($"chatrooms:{channelId}:{roomId}"));
            if (!this._currentlyJoiningChannels)
                this.QueueingJoinCheck();
        }
        /// <summary>
        /// Returns a JoinedChannel object using a passed string/>.
        /// </summary>
        /// <param name="channel">String channel to search for.</param>
        public JoinedChannel GetJoinedChannel(String channel)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            if (this.JoinedChannels.Count == 0)
                throw new BadStateException("Must be connected to at least one channel.");
            return this._joinedChannelManager.GetJoinedChannel(channel);
        }

        /// <summary>
        /// Leaves (PART) the Twitch IRC chat of <paramref name="channel"/>.
        /// </summary>
        /// <param name="channel">The channel to leave.</param>
        /// <returns>True is returned if the passed channel was found, false if channel not found.</returns>
        public void LeaveChannel(String channel)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            // Channel MUST be lower case
            channel = channel.ToLower();
            this.Log($"Leaving channel: {channel}");
            var joinedChannel = this._joinedChannelManager.GetJoinedChannel(channel);
            if (joinedChannel != null)
                this._client.Send(Rfc2812.Part($"#{channel}"));
        }

        public void LeaveRoom(String channelId, String roomId)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            var room = $"chatrooms:{channelId}:{roomId}";
            this.Log($"Leaving channel: {room}");
            var joinedChannel = this._joinedChannelManager.GetJoinedChannel(room);
            if (joinedChannel != null)
                this._client.Send(Rfc2812.Part($"#{room}"));
        }

        /// <summary>
        /// Leaves (PART) the Twitch IRC chat of <paramref name="channel"/>.
        /// </summary>
        /// <param name="channel">The JoinedChannel object to leave.</param>
        /// <returns>True is returned if the passed channel was found, false if channel not found.</returns>
        public void LeaveChannel(JoinedChannel channel)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            this.LeaveChannel(channel.Channel);
        }

        #endregion

        /// <summary>
        /// This method allows firing the message parser with a custom irc string allowing for easy testing
        /// </summary>
        /// <param name="rawIrc">This should be a raw IRC message resembling one received from Twitch IRC.</param>
        public void OnReadLineTest(String rawIrc)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            this.HandleIrcMessage(this._ircParser.ParseIrcMessage(rawIrc));
        }

        #region Client Events

        private void _client_OnWhisperThrottled(Object sender, OnWhisperThrottledEventArgs e)
        {
            this.OnWhisperThrottled?.Invoke(sender, e);
        }

        private void _client_OnMessageThrottled(Object sender, OnMessageThrottledEventArgs e)
        {
            this.OnMessageThrottled?.Invoke(sender, e);
        }

        private void _client_OnError(Object sender, OnErrorEventArgs e)
        {
            this.OnError?.Invoke(sender, e);
        }

        private void _client_OnFatality(Object sender, OnFatalErrorEventArgs e)
        {
            this.OnConnectionError?.Invoke(this, new OnConnectionErrorArgs { BotUsername = this.TwitchUsername, Error = new ErrorEvent { Message = e.Reason } });
        }

        private void _client_OnDisconnected(Object sender, OnDisconnectedEventArgs e)
        {
            this.OnDisconnected?.Invoke(sender, e);
            this._joinedChannelManager.Clear();
        }

        private void _client_OnReconnected(Object sender, OnReconnectedEventArgs e)
        {
            this.OnReconnected?.Invoke(sender, e);
        }

        private void _client_OnMessage(Object sender, OnMessageEventArgs e)
        {
            var stringSeparators = new[] { "\r\n" };
            var lines = e.Message.Split(stringSeparators, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (line.Length <= 1)
                    continue;

                this.Log($"Received: {line}");
                this.OnSendReceiveData?.Invoke(this, new OnSendReceiveDataArgs { Direction = SendReceiveDirection.Received, Data = line });
                this.HandleIrcMessage(this._ircParser.ParseIrcMessage(line));
            }
        }

        private void _client_OnConnected(Object sender, Object e)
        {
            this._client.Send(Rfc2812.Pass(this.ConnectionCredentials.TwitchOAuth));
            this._client.Send(Rfc2812.Nick(this.ConnectionCredentials.TwitchUsername));
            this._client.Send(Rfc2812.User(this.ConnectionCredentials.TwitchUsername, 0, this.ConnectionCredentials.TwitchUsername));

            this._client.Send("CAP REQ twitch.tv/membership");
            this._client.Send("CAP REQ twitch.tv/commands");
            this._client.Send("CAP REQ twitch.tv/tags");

            if (this._autoJoinChannel != null)
            {
                this.JoinChannel(this._autoJoinChannel);
            }
        }

        #endregion

        #region Joining Stuff

        private void QueueingJoinCheck()
        {
            if (this._joinChannelQueue.Count > 0)
            {
                this._currentlyJoiningChannels = true;
                var channelToJoin = this._joinChannelQueue.Dequeue();
                this.Log($"Joining channel: {channelToJoin.Channel}");
                // important we set channel to lower case when sending join message
                this._client.Send(Rfc2812.Join($"#{channelToJoin.Channel.ToLower()}"));
                this._joinedChannelManager.AddJoinedChannel(new JoinedChannel(channelToJoin.Channel));
                this.StartJoinedChannelTimer(channelToJoin.Channel);
            }
            else
            {
                this.Log("Finished channel joining queue.");
            }
        }

        private void StartJoinedChannelTimer(String channel)
        {
            if (this._joinTimer == null)
            {
                this._joinTimer = new System.Timers.Timer(1000);
                this._joinTimer.Elapsed += this.JoinChannelTimeout;
                this._awaitingJoins = new List<KeyValuePair<String, DateTime>>();
            }
            this._awaitingJoins.Add(new KeyValuePair<String, DateTime>(channel, DateTime.Now));
            if (!this._joinTimer.Enabled)
                this._joinTimer.Start();
        }

        private void JoinChannelTimeout(Object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this._awaitingJoins.Any())
            {
                var expiredChannels = this._awaitingJoins.Where(x => (DateTime.Now - x.Value).TotalSeconds > 5).ToList();
                if (expiredChannels.Any())
                {
                    this._awaitingJoins.RemoveAll(x => (DateTime.Now - x.Value).TotalSeconds > 5);
                    foreach (var expiredChannel in expiredChannels)
                    {
                        this._joinedChannelManager.RemoveJoinedChannel(expiredChannel.Key.ToLowerInvariant());
                        this.OnFailureToReceiveJoinConfirmation?.Invoke(this, new OnFailureToReceiveJoinConfirmationArgs { Exception = new FailureToReceiveJoinConfirmationException(expiredChannel.Key) });
                    }
                }
            }
            else
            {
                this._joinTimer.Stop();
                this._currentlyJoiningChannels = false;
                this.QueueingJoinCheck();
            }
        }

        #endregion

        #region IrcMessage Handling

        private void HandleIrcMessage(IrcMessage ircMessage)
        {
            if (ircMessage.Message.Contains("Login authentication failed"))
            {
                this.OnIncorrectLogin?.Invoke(this, new OnIncorrectLoginArgs { Exception = new ErrorLoggingInException(ircMessage.ToString(), this.TwitchUsername) });
            }

            switch (ircMessage.Command)
            {
                case IrcCommand.PrivMsg:
                    this.HandlePrivMsg(ircMessage);
                    return;
                case IrcCommand.Notice:
                    this.HandleNotice(ircMessage);
                    break;
                case IrcCommand.Ping:
                    if (!this.DisableAutoPong)
                        this.SendRaw("PONG");
                    return;
                case IrcCommand.Pong:
                    return;
                case IrcCommand.Join:
                    this.HandleJoin(ircMessage);
                    break;
                case IrcCommand.Part:
                    this.HandlePart(ircMessage);
                    break;
                case IrcCommand.HostTarget:
                    this.HandleHostTarget(ircMessage);
                    break;
                case IrcCommand.ClearChat:
                    this.HandleClearChat(ircMessage);
                    break;
                case IrcCommand.UserState:
                    this.HandleUserState(ircMessage);
                    break;
                case IrcCommand.GlobalUserState:
                    break;
                case IrcCommand.RPL_001:
                    break;
                case IrcCommand.RPL_002:
                    break;
                case IrcCommand.RPL_003:
                    break;
                case IrcCommand.RPL_004:
                    this.Handle004();
                    break;
                case IrcCommand.RPL_353:
                    this.Handle353(ircMessage);
                    break;
                case IrcCommand.RPL_366:
                    this.Handle366();
                    break;
                case IrcCommand.RPL_372:
                    break;
                case IrcCommand.RPL_375:
                    break;
                case IrcCommand.RPL_376:
                    break;
                case IrcCommand.Whisper:
                    this.HandleWhisper(ircMessage);
                    break;
                case IrcCommand.RoomState:
                    this.HandleRoomState(ircMessage);
                    break;
                case IrcCommand.Reconnect:
                    this.Reconnect();
                    break;
                case IrcCommand.UserNotice:
                    this.HandleUserNotice(ircMessage);
                    break;
                case IrcCommand.Mode:
                    this.HandleMode(ircMessage);
                    break;
                case IrcCommand.Unknown:
                    this.OnUnaccountedFor?.Invoke(this, new OnUnaccountedForArgs { BotUsername = this.TwitchUsername, Channel = null, Location = "HandleIrcMessage", RawIRC = ircMessage.ToString() });
                    this.Log($"Unaccounted for: {ircMessage.ToString()}");
                    break;
                default:
                    this.OnUnaccountedFor?.Invoke(this, new OnUnaccountedForArgs { BotUsername = this.TwitchUsername, Channel = null, Location = "HandleIrcMessage", RawIRC = ircMessage.ToString() });
                    this.Log($"Unaccounted for: {ircMessage.ToString()}");
                    break;
            }
        }

        #region IrcCommand Handling

        private void HandlePrivMsg(IrcMessage ircMessage)
        {
            if (ircMessage.Hostmask.Equals("jtv!jtv@jtv.tmi.twitch.tv"))
            {
                var hostNotification = new BeingHostedNotification(this.TwitchUsername, ircMessage);
                this.OnBeingHosted?.Invoke(this, new OnBeingHostedArgs { BeingHostedNotification = hostNotification });
                return;
            }

            var chatMessage = new ChatMessage(this.TwitchUsername, ircMessage, ref this._channelEmotes, this.WillReplaceEmotes);
            foreach (var joinedChannel in this.JoinedChannels.Where(x => String.Equals(x.Channel, ircMessage.Channel, StringComparison.InvariantCultureIgnoreCase)))
                joinedChannel.HandleMessage(chatMessage);
            this.OnMessageReceived?.Invoke(this, new OnMessageReceivedArgs { ChatMessage = chatMessage });

            if (this._chatCommandIdentifiers != null && this._chatCommandIdentifiers.Count != 0 && !String.IsNullOrEmpty(chatMessage.Message))
            {
                if (this._chatCommandIdentifiers.Contains(chatMessage.Message[0]))
                {
                    var chatCommand = new ChatCommand(chatMessage);
                    this.OnChatCommandReceived?.Invoke(this, new OnChatCommandReceivedArgs { Command = chatCommand });
                    return;
                }
            }
        }

        private void HandleNotice(IrcMessage ircMessage)
        {
            if (ircMessage.Message.Contains("Improperly formatted auth"))
            {
                this.OnIncorrectLogin?.Invoke(this, new OnIncorrectLoginArgs { Exception = new ErrorLoggingInException(ircMessage.ToString(), this.TwitchUsername) });
                return;
            }

            var success = ircMessage.Tags.TryGetValue(Tags.MsgId, out var msgId);
            if (!success)
            {
                this.OnUnaccountedFor?.Invoke(this, new OnUnaccountedForArgs { BotUsername = this.TwitchUsername, Channel = ircMessage.Channel, Location = "NoticeHandling", RawIRC = ircMessage.ToString() });
                this.Log($"Unaccounted for: {ircMessage.ToString()}");
            }

            switch (msgId)
            {
                case MsgIds.ColorChanged:
                    this.OnChatColorChanged?.Invoke(this, new OnChatColorChangedArgs { Channel = ircMessage.Channel });
                    break;
                case MsgIds.HostOn:
                    this.OnNowHosting?.Invoke(this, new OnNowHostingArgs { Channel = ircMessage.Channel, HostedChannel = ircMessage.Message.Split(' ')[2].Replace(".", "") });
                    break;
                case MsgIds.HostOff:
                    this.OnHostLeft?.Invoke(this, null);
                    break;
                case MsgIds.ModeratorsReceived:
                    this.OnModeratorsReceived?.Invoke(this, ircMessage.Message.Contains("There are no moderators of this room.")
                            ? new OnModeratorsReceivedArgs
                            {
                                Channel = ircMessage.Channel,
                                Moderators = new List<String>()
                            }
                            : new OnModeratorsReceivedArgs
                            {
                                Channel = ircMessage.Channel,
                                Moderators = ircMessage.Message.Replace(" ", "").Split(':')[1].Split(',').ToList()
                            });
                    break;
                case MsgIds.NoPermission:
                    this.OnNoPermissionError?.Invoke(this, null);
                    break;
                case MsgIds.RaidErrorSelf:
                    this.OnSelfRaidError?.Invoke(this, null);
                    break;
                case MsgIds.RaidNoticeMature:
                    this.OnRaidedChannelIsMatureAudience?.Invoke(this, null);
                    break;
                case MsgIds.MsgChannelSuspended:
                    this._awaitingJoins.RemoveAll(x => x.Key.ToLower() == ircMessage.Channel);
                    this._joinedChannelManager.RemoveJoinedChannel(ircMessage.Channel);
                    this.QueueingJoinCheck();
                    this.OnFailureToReceiveJoinConfirmation?.Invoke(this, new OnFailureToReceiveJoinConfirmationArgs {
                        Exception = new FailureToReceiveJoinConfirmationException(ircMessage.Channel, ircMessage.Message)
                        });
                    break;
                default:
                    this.OnUnaccountedFor?.Invoke(this, new OnUnaccountedForArgs { BotUsername = this.TwitchUsername, Channel = ircMessage.Channel, Location = "NoticeHandling", RawIRC = ircMessage.ToString() });
                    this.Log($"Unaccounted for: {ircMessage.ToString()}");
                    break;
            }
        }

        private void HandleJoin(IrcMessage ircMessage)
        {
            this.OnUserJoined?.Invoke(this, new OnUserJoinedArgs { Channel = ircMessage.Channel, Username = ircMessage.User });
        }

        private void HandlePart(IrcMessage ircMessage)
        {
            if (String.Equals(this.TwitchUsername, ircMessage.User, StringComparison.InvariantCultureIgnoreCase))
            {
                this._joinedChannelManager.RemoveJoinedChannel(ircMessage.Channel);
                this._hasSeenJoinedChannels.Remove(ircMessage.Channel);
                this.OnLeftChannel?.Invoke(this, new OnLeftChannelArgs { BotUsername = this.TwitchUsername, Channel = ircMessage.Channel });
            }
            else
            {
                this.OnUserLeft?.Invoke(this, new OnUserLeftArgs { Channel = ircMessage.Channel, Username = ircMessage.User });
            }
        }

        private void HandleHostTarget(IrcMessage ircMessage)
        {
            if (ircMessage.Message.StartsWith("-"))
            {
                var hostingStopped = new HostingStopped(ircMessage);
                this.OnHostingStopped?.Invoke(this, new OnHostingStoppedArgs { HostingStopped = hostingStopped });
            }
            else
            {
                var hostingStarted = new HostingStarted(ircMessage);
                this.OnHostingStarted?.Invoke(this, new OnHostingStartedArgs { HostingStarted = hostingStarted });
            }
        }

        private void HandleClearChat(IrcMessage ircMessage)
        {
            if (String.IsNullOrWhiteSpace(ircMessage.Message))
            {
                this.OnChatCleared?.Invoke(this, new OnChatClearedArgs { Channel = ircMessage.Channel });
                return;
            }

            var successBanDuration = ircMessage.Tags.TryGetValue(Tags.BanDuration, out _);
            if (successBanDuration)
            {
                var userTimeout = new UserTimeout(ircMessage);
                this.OnUserTimedout?.Invoke(this, new OnUserTimedoutArgs { UserTimeout = userTimeout });
                return;
            }

            var userBan = new UserBan(ircMessage);
            this.OnUserBanned?.Invoke(this, new OnUserBannedArgs { UserBan = userBan });
        }

        private void HandleUserState(IrcMessage ircMessage)
        {
            var userState = new UserState(ircMessage);
            if (!this._hasSeenJoinedChannels.Contains(userState.Channel.ToLowerInvariant()))
            {
                this._hasSeenJoinedChannels.Add(userState.Channel.ToLowerInvariant());
                this.OnUserStateChanged?.Invoke(this, new OnUserStateChangedArgs { UserState = userState });
            }
            else
                this.OnMessageSent?.Invoke(this, new OnMessageSentArgs { SentMessage = new SentMessage(userState, this._lastMessageSent) });
        }

        private void Handle004()
        {
            this.OnConnected?.Invoke(this, new OnConnectedArgs { AutoJoinChannel = this._autoJoinChannel, BotUsername = this.TwitchUsername });
        }

        private void Handle353(IrcMessage ircMessage)
        {
            if (String.Equals(ircMessage.Channel, this.TwitchUsername, StringComparison.InvariantCultureIgnoreCase))
            {
                this.OnExistingUsersDetected?.Invoke(this, new OnExistingUsersDetectedArgs { Channel = ircMessage.Channel, Users = ircMessage.Message.Split(' ').ToList() });
            }
        }

        private void Handle366()
        {
            this._currentlyJoiningChannels = false;
            this.QueueingJoinCheck();
        }

        private void HandleWhisper(IrcMessage ircMessage)
        {
            var whisperMessage = new WhisperMessage(ircMessage, this.TwitchUsername);
            this.PreviousWhisper = whisperMessage;
            this.OnWhisperReceived?.Invoke(this, new OnWhisperReceivedArgs { WhisperMessage = whisperMessage });

            if (this._whisperCommandIdentifiers != null && this._whisperCommandIdentifiers.Count != 0 && !String.IsNullOrEmpty(whisperMessage.Message))
                if (this._whisperCommandIdentifiers.Contains(whisperMessage.Message[0]))
                {
                    var whisperCommand = new WhisperCommand(whisperMessage);
                    this.OnWhisperCommandReceived?.Invoke(this, new OnWhisperCommandReceivedArgs { Command = whisperCommand });
                    return;
                }
            this.OnUnaccountedFor?.Invoke(this, new OnUnaccountedForArgs { BotUsername = this.TwitchUsername, Channel = ircMessage.Channel, Location = "WhispergHandling", RawIRC = ircMessage.ToString() });
            this.Log($"Unaccounted for: {ircMessage.ToString()}");
        }

        private void HandleRoomState(IrcMessage ircMessage)
        {
            if (ircMessage.Tags.ContainsKey(Tags.SubsOnly) && ircMessage.Tags.ContainsKey(Tags.Slow))
            {
                var channel = this._awaitingJoins.FirstOrDefault(x => x.Key == ircMessage.Channel);
                this._awaitingJoins.Remove(channel);

                this.OnJoinedChannel?.Invoke(this, new OnJoinedChannelArgs { BotUsername = this.TwitchUsername, Channel = ircMessage.Channel });
                if (this.OnBeingHosted != null)
                if (ircMessage.Channel.ToLowerInvariant() != this.TwitchUsername && !this.OverrideBeingHostedCheck)
                    this.Log("[OnBeingHosted] OnBeingHosted will only be fired while listening to this event as the broadcaster's channel. You do not appear to be connected as the broadcaster. To hide this warning, set TwitchClient property OverrideBeingHostedCheck to true.");
            }

            this._channelState.Update(ircMessage);
            this.OnChannelStateChanged?.Invoke(this, new OnChannelStateChangedArgs { ChannelState =this._channelState, Channel = ircMessage.Channel });
        }

        private void HandleUserNotice(IrcMessage ircMessage)
        {
            var successMsgId = ircMessage.Tags.TryGetValue(Tags.MsgId, out var msgId);
            if (!successMsgId)
            {
                this.OnUnaccountedFor?.Invoke(this, new OnUnaccountedForArgs { BotUsername = this.TwitchUsername, Channel = ircMessage.Channel, Location = "UserNoticeHandling", RawIRC = ircMessage.ToString() });
                this.Log($"Unaccounted for: {ircMessage.ToString()}");
                return;
            }

            switch (msgId)
            {
                case MsgIds.Raid:
                    var raidNotification = new RaidNotification(ircMessage);
                    this.OnRaidNotification?.Invoke(this, new OnRaidNotificationArgs { Channel = ircMessage.Channel, RaidNotificaiton = raidNotification });
                    break;
                case MsgIds.ReSubscription:
                    var resubscriber = new ReSubscriber(ircMessage);
                    this.OnReSubscriber?.Invoke(this, new OnReSubscriberArgs { ReSubscriber = resubscriber, Channel = ircMessage.Channel });
                    break;
                case MsgIds.Ritual:
                    var successRitualName = ircMessage.Tags.TryGetValue(Tags.MsgParamRitualName, out var ritualName);
                    if (!successRitualName)
                    {
                        this.OnUnaccountedFor?.Invoke(this, new OnUnaccountedForArgs { BotUsername = this.TwitchUsername, Channel = ircMessage.Channel, Location = "UserNoticeRitualHandling", RawIRC = ircMessage.ToString() });
                        this.Log($"Unaccounted for: {ircMessage.ToString()}");
                        return;
                    }
                    switch (ritualName)
                    {
                        case "new_chatter": // In case there will be more Rituals we should do a "string enum" for them too but for now this will do
                            this.OnRitualNewChatter?.Invoke(this, new OnRitualNewChatterArgs { RitualNewChatter = new RitualNewChatter(ircMessage) });
                            break;
                        default:
                            this.OnUnaccountedFor?.Invoke(this, new OnUnaccountedForArgs { BotUsername = this.TwitchUsername, Channel = ircMessage.Channel, Location = "UserNoticeHandling", RawIRC = ircMessage.ToString() });
                            this.Log($"Unaccounted for: {ircMessage.ToString()}");
                            break;
                    }
                    break;
                case MsgIds.SubGift:
                    var giftedSubscription = new GiftedSubscription(ircMessage);
                    this.OnGiftedSubscription?.Invoke(this, new OnGiftedSubscriptionArgs { GiftedSubscription = giftedSubscription, Channel = ircMessage.Channel });
                    break;
                case MsgIds.CommunitySubscription:
                    var communitySubscription = new CommunitySubscription(ircMessage);
                    this.OnCommunitySubscription?.Invoke(this, new OnCommunitySubscriptionArgs { GiftedSubscription = communitySubscription, Channel = ircMessage.Channel });
                    break;
                case MsgIds.Subscription:
                    var subscriber = new Subscriber(ircMessage);
                    this.OnNewSubscriber?.Invoke(this, new OnNewSubscriberArgs { Subscriber = subscriber, Channel = ircMessage.Channel });
                    break;
                default:
                    this.OnUnaccountedFor?.Invoke(this, new OnUnaccountedForArgs { BotUsername = this.TwitchUsername, Channel = ircMessage.Channel, Location = "UserNoticeHandling", RawIRC = ircMessage.ToString() });
                    this.Log($"Unaccounted for: {ircMessage.ToString()}");
                    break;
            }
        }

        private void HandleMode(IrcMessage ircMessage)
        {
            if (ircMessage.Message.StartsWith("+o"))
            {
                this.OnModeratorJoined?.Invoke(this, new OnModeratorJoinedArgs { Channel = ircMessage.Channel, Username = ircMessage.Message.Split(' ')[1] });
                return;
            }

            if (ircMessage.Message.StartsWith("-o"))
            {
                this.OnModeratorLeft?.Invoke(this, new OnModeratorLeftArgs { Channel = ircMessage.Channel, Username = ircMessage.Message.Split(' ')[1] });
            }
        }

        #endregion

        #endregion

        private void Log(String message, Boolean includeDate = false, Boolean includeTime = false)
        {
            String dateTimeStr;
            if (includeDate && includeTime)
                dateTimeStr = $"{DateTime.UtcNow}";
            else if (includeDate)
                dateTimeStr = $"{DateTime.UtcNow.ToShortDateString()}";
            else
                dateTimeStr = $"{DateTime.UtcNow.ToShortTimeString()}";

            if (includeDate || includeTime)
                this._logger?.LogInformation($"[TwitchLib, {Assembly.GetExecutingAssembly().GetName().Version} - {dateTimeStr}] {message}");
            else
                this._logger?.LogInformation($"[TwitchLib, {Assembly.GetExecutingAssembly().GetName().Version}] {message}");

            this.OnLog?.Invoke(this, new OnLogArgs { BotUsername = this.ConnectionCredentials?.TwitchUsername, Data = message, DateTime = DateTime.UtcNow });
        }
        
        private void LogError(String message, Boolean includeDate = false, Boolean includeTime = false)
        {
            String dateTimeStr;
            if (includeDate && includeTime)
                dateTimeStr = $"{DateTime.UtcNow}";
            else if (includeDate)
                dateTimeStr = $"{DateTime.UtcNow.ToShortDateString()}";
            else
                dateTimeStr = $"{DateTime.UtcNow.ToShortTimeString()}";

            if (includeDate || includeTime)
                this._logger?.LogError($"[TwitchLib, {Assembly.GetExecutingAssembly().GetName().Version} - {dateTimeStr}] {message}");
            else
                this._logger?.LogError($"[TwitchLib, {Assembly.GetExecutingAssembly().GetName().Version}] {message}");

            this.OnLog?.Invoke(this, new OnLogArgs { BotUsername = this.ConnectionCredentials?.TwitchUsername, Data = message, DateTime = DateTime.UtcNow });
        }
        
        public void SendQueuedItem(String message)
        {
            if (!this.IsInitialized) HandleNotInitialized();
            this._client.Send(message);
        }

        protected static void HandleNotInitialized()
        {
            throw new ClientNotInitializedException("The twitch client has not been initialized and cannot be used. Please call Initialize();");
        }

        protected static void HandleNotConnected()
        {
            throw new ClientNotConnectedException("In order to perform this action, the client must be connected to Twitch. To confirm connection, try performing this action in or after the OnConnected event has been fired.");
        }

        public void Dispose()
        {
            this._joinTimer?.Dispose();
            this._client?.Dispose();
        }
    }
}
