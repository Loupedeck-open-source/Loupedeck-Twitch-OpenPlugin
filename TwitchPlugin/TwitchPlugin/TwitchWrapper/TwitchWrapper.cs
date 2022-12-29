namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TwitchLib.Api;
    using TwitchLib.Api.Core;
    using TwitchLib.Api.Core.Exceptions;
    using TwitchLib.Api.Core.Models.Undocumented.Chatters;
    using TwitchLib.Client;
    using TwitchLib.Client.Events;
    using TwitchLib.Client.Models;
    using TwitchLib.Communication.Events;

    public class TwitchWrapper : IDisposable
    {
        private readonly Object _viewersLock;

        private TwitchApi _twitchApi;
        private TwitchClient _twitchClient;
        private UserInfo _userInfo;
        private ChannelState _channelState;
        private Boolean _initializationBlocked;
        private CancellationTokenSource _viewerUpdaterCancellationTokenSource = new CancellationTokenSource();
        private Boolean _initialized;

        public EventHandler AccessTokenExpired { get; set; }
        public EventHandler<String> Connected { get; set; }
        public EventHandler Disconnected { get; set; }
        public EventHandler<String> Reconnected { get; set; }
        public EventHandler<String> ConnectionError { get; set; }
        public EventHandler<Exception> Error { get; set; }
        public EventHandler<(String, Exception)> IncorrectLogin { get; set; }
        public EventHandler<HashSet<String>> ChattersChanged { get; set; }
        public EventHandler ChannelStatusChanged { get; set; }
        public EventHandler ViewersChanged { get; set; }

        public HashSet<String> Chatters { get; }
        public Int32 CurrentViewersCount { get; private set; }

        public Boolean IsConnected => this._twitchClient?.IsConnected == true;
        public Boolean IsSubOnly => this._channelState?.SubOnly == true;
        public Boolean IsEmoteOnly => this._channelState?.EmoteOnly == true;
        public Int32 SlowMode => this._channelState?.SlowMode ?? 0;
        public TimeSpan FollowersOnly => this._channelState?.FollowersOnly ?? TimeSpan.Zero;

        public Boolean IsSlowMode => this.SlowMode != 0;
        public Boolean IsFollowersOnly => this.FollowersOnly != TimeSpan.Zero;

        public event EventHandler<EventArgs> AppEvtChatEmotesOnlyOn;
        public event EventHandler<EventArgs> AppEvtChatEmotesOnlyOff;

        public event EventHandler<EventArgs> AppEvtChatSubscribersOnlyOn;
        public event EventHandler<EventArgs> AppEvtChatSubscribersOnlyOff;

        public event EventHandler<EventArgs> AppEvtChatFollowersOnlyOn;
        public event EventHandler<EventArgs> AppEvtChatFollowersOnlyOff;

        public event EventHandler<EventArgs> AppEvtChatSlowModeOn;
        public event EventHandler<EventArgs> AppEvtChatSlowModeOff;


        public void AppToggleEmotesOnly()
        {
            this.SendMessage(this.IsEmoteOnly ? ".emoteonlyoff" : ".emoteonly");
        }
        public void AppToggleSubscribersOnly()
        {
            this.SendMessage(this.IsSubOnly? ".subscribersoff" : ".subscribers");
        }
        public void AppToggleFollowersOnly()
        {
            this.SendMessage(this.IsFollowersOnly ? ".followersoff" : ".followerson 10m");
        }
        public void AppToggleSlowMode()
        {
            this.SendMessage(TwitchPlugin.Proxy.IsSlowMode ? ".slowoff" : ".slow 30");
        }

        public TwitchWrapper()
        {
            this._viewersLock = new Object();
            this.Chatters = new HashSet<String>();
        }

        public void Initialize(String clientId, String clientSecret)
        {
            this._twitchApi = new TwitchApi(new Logger(), settings: new ApiSettings
            {
                ClientId = clientId,
                Secret = clientSecret,
                SkipAutoServerTokenGeneration = true,
                SkipDynamicScopeValidation = true
            });

            this._initialized = true;
        }

        public async Task Connect(String accessToken)
        {
            TwitchPlugin.PluginLog.Info("TwitchWrapper Connect");

            if (!this._initialized)
            {
                throw new Exception("TwitchWrapper not initialized.");
            }

            try
            {
                this._twitchApi.Settings.AccessToken = accessToken;
                if (this._twitchClient?.IsConnected == true)
                {
                    this.StopViewersUpdater();
                    this._twitchClient.OnDisconnected -= this.OnDisconnected;
                    this._twitchClient.Disconnect();
                    this._twitchClient.Dispose();
                }

                await this.InitializeTwitchClient(accessToken);
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e,$"TwitchWrapper Connect error: {e.Message}");
                throw;
            }
        }

        public void Disconnect()
        {
            TwitchPlugin.PluginLog.Info("TwitchWrapper Disconnect");

            try
            {
                if (this._twitchApi != null)
                {
                    this._twitchApi.Settings.AccessToken = null;
                }
               
                if (this._twitchClient == null)
                {
                    return;
                }

                this.StopViewersUpdater();
                this._twitchClient.OnDisconnected -= this.OnDisconnected;
                this._twitchClient.Disconnect();
                this._twitchClient.Dispose();
                this.CurrentViewersCount = 0;
                this.Chatters.Clear();
                this.ChattersChanged?.Invoke(this, this.Chatters);
                this.ViewersChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e,$"TwitchWrapper Disconnect error: {e.Message}");
                throw;
            }
        }

        public void SendMessage(String message)
        {
            try
            {
                if (this._twitchClient.JoinedChannels.All(c => !c.Channel.Equals(this._userInfo.Login)))
                {
                    this.JoinChannel(this._userInfo.Login, () =>
                    {
                        this._twitchClient.SendMessage(this._userInfo.Login, message);
                    });
                    return;
                }

                this._twitchClient.SendMessage(this._userInfo.Login, message);
            }
            catch (TokenExpiredException)
            {
                this.AccessTokenExpired?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task CreateMarkerCommandAsync()
        {
            try
            {
                await this._twitchApi.Helix.Streams.CreateMarkerAsync(this._userInfo.Id.ToString(), "Loupedeck Marker");
            }
            catch (TokenExpiredException)
            {
                this.AccessTokenExpired?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Warning(e,"TwitchPlugin.CreateMarkerCommandAsync error: " + e.Message);
            }
        }

        public async Task CreateClipCommandAsync()
        {
            try
            {
                var clip = (await this._twitchApi.Helix.Clips.CreateClipAsync(this._userInfo.Id.ToString())).CreatedClips.First();
                this.SendMessage(clip.EditUrl);
            }
            catch (TokenExpiredException)
            {
                this.AccessTokenExpired?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Warning(e, "TwitchPlugin.CreateClipCommandAsync error: " + e.Message);
            }
        }

        public void Dispose()
        {
            this.StopViewersUpdater();
            this._twitchClient?.Dispose();
            this._viewerUpdaterCancellationTokenSource?.Dispose();
        }

        private async Task InitializeTwitchClient(String accessToken)
        {
            TwitchPlugin.PluginLog.Info("TwitchWrapper Initialize");

            if (this._initializationBlocked)
            {
                TwitchPlugin.PluginLog.Info("Can`t initialize TwitchAPI, initialization is blocked by another init");
                return;
            }

            this._initializationBlocked = true;

            try
            {
                TwitchPlugin.PluginLog.Info("Initializing TwitchAPI");
                this._twitchClient = new TwitchClient(logger: new Logger());
                this._twitchClient.OnConnected += this.OnConnected;
                this._twitchClient.OnJoinedChannel += this.OnJoinedChannel;
                this._twitchClient.OnChannelStateChanged += this.OnChannelStateChanged;
                this._twitchClient.OnUserJoined += this.OnUserJoined;
                this._twitchClient.OnUserLeft += this.OnUserLeft;
                this._twitchClient.OnReconnected += this.OnReconnected;
                this._twitchClient.OnIncorrectLogin += this.OnIncorrectLogin;
                this._twitchClient.OnConnectionError += this.OnConnectionError;
                this._twitchClient.OnError += this.OnError;
                this._userInfo = GetUserInfo(accessToken);
                this._twitchClient.Initialize(
                    new ConnectionCredentials(this._userInfo.Login, accessToken),
                    this._userInfo.Login);
                this._twitchClient.Connect();
                await this.FetchChattersAsync();
            }
            catch (TaskCanceledException)
            {
                TwitchPlugin.PluginLog.Info("Initialize canceled.");
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e, "Failed to initialize Twitch API.");
            }
            finally
            {
                //add wait all
                this._initializationBlocked = false;
            }
        }

        private void JoinChannel(String channel, Action callback = null)
        {
            void ChannelJoined(Object sender, OnChannelStateChangedArgs e)
            {
                if (!e.Channel.Equals(channel))
                {
                    return;
                }

                this._twitchClient.OnChannelStateChanged -= ChannelJoined;
                callback?.Invoke();
            }

            this._twitchClient.OnChannelStateChanged += ChannelJoined;
            this._twitchClient.JoinChannel(channel);
        }

        private void OnConnectionError(Object sender, OnConnectionErrorArgs e)
        {
            this.StopViewersUpdater();
            this.ConnectionError?.Invoke(sender, e.Error.Message);
        }

        private void OnDisconnected(Object sender, OnDisconnectedEventArgs e)
        {
            this.StopViewersUpdater();
            this.CurrentViewersCount = 0;
            this.Chatters.Clear();
            this.ChattersChanged?.Invoke(this, this.Chatters);
            this.ViewersChanged?.Invoke(this, EventArgs.Empty);
            this.Disconnected?.Invoke(sender, EventArgs.Empty);
        }

        private void OnIncorrectLogin(Object sender, OnIncorrectLoginArgs e)
        {
            this.IncorrectLogin?.Invoke(sender, (e.Exception.Username, e.Exception));
            this._twitchClient.Disconnect();
        }

        private void OnConnected(Object sender, OnConnectedArgs e)
        {
            // need to ensure that we only subscribe once
            this._twitchClient.OnDisconnected -= this.OnDisconnected;
            this._twitchClient.OnDisconnected += this.OnDisconnected;
            this.UpdateViewersAsync().ConfigureAwait(false);
            this.InitViewersUpdater();
            this.Connected?.Invoke(sender, e.BotUsername);
        }

        private void OnReconnected(Object sender, OnReconnectedEventArgs e)
        {
            // need to ensure that we only subscribe once
            this._twitchClient.OnDisconnected -= this.OnDisconnected;
            this._twitchClient.OnDisconnected += this.OnDisconnected;
            this.InitViewersUpdater();
            this.Reconnected?.Invoke(sender, this._userInfo.Login);
        }

        private void OnError(Object sender, OnErrorEventArgs e)
        {
            this.Error?.Invoke(sender, e.Exception);
        }

        private void OnChannelStateChanged(Object sender, OnChannelStateChangedArgs e)
        {
            if (e.Channel != this._userInfo.Login)
            {
                return;
            }

            if (this._twitchClient.JoinedChannels.All(c => !c.Channel.Equals(this._userInfo.Login)))
            {
                this._twitchClient.JoinChannel(this._userInfo.Login);
            }

            var prev_emote = this.IsEmoteOnly;
            var prev_sub = this.IsSubOnly;
            var prev_follow = this.FollowersOnly;
            var prev_slow = this.SlowMode;

            this._channelState = e.ChannelState;

            if(this.IsEmoteOnly != prev_emote)
            {   
                if(this.IsEmoteOnly)
                { 
                    this.AppEvtChatEmotesOnlyOn.Invoke(this, e);
                }
                else
                {
                    this.AppEvtChatEmotesOnlyOff.Invoke(this, e);
                }
            }

            if (this.IsSubOnly != prev_sub)
            {
                if (this.IsSubOnly)
                {
                    this.AppEvtChatSubscribersOnlyOn.Invoke(this, e);
                }
                else
                {
                    this.AppEvtChatSubscribersOnlyOff.Invoke(this, e);
                }
            }
/*
 *  This is not yet needed
            if (this.FollowersOnly != prev_follow)
            {
                if (this.IsFollowersOnly)
                {
                    this.AppEvtChatFollowersOnlyOn.Invoke(this, e);
                }
                else
                {
                    this.AppEvtChatFollowersOnlyOff.Invoke(this, e);
                }
            }

            if (this.SlowMode != prev_slow)
            {
                if (this.IsSlowMode)
                {
                    this.AppEvtChatSlowModeOn.Invoke(this, e);
                }
                else
                {
                    this.AppEvtChatSlowModeOff.Invoke(this, e);
                }
            }
*/
            this.ChannelStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnJoinedChannel(Object sender, OnJoinedChannelArgs e)
        {
            TwitchPlugin.PluginLog.Info($"Joined channel: {e.Channel}");
        }

        private void OnUserLeft(Object sender, OnUserLeftArgs e)
        {
            lock (this._viewersLock)
            {
                if (e.Username != this._userInfo.Login)
                {
                    this.Chatters.Remove(e.Username);
                }
            }

            this.ChattersChanged?.Invoke(this, this.Chatters);
        }

        private void OnUserJoined(Object sender, OnUserJoinedArgs e)
        {
            lock (this._viewersLock)
            {
                if (e.Username != this._userInfo.Login)
                {
                    this.Chatters.Add(e.Username);
                }
            }

            this.ChattersChanged?.Invoke(this, this.Chatters);
        }

        // This function uses an undocumented endpoint, meaning it can break at any time.
        // https://discuss.dev.twitch.tv/t/getting-current-viewers-of-a-twitch-stream
        private async Task FetchChattersAsync()
        {
            this.Chatters.Clear();
            this.ChattersChanged?.Invoke(this, this.Chatters);

            List<ChatterFormatted> chatters = null;

            var fetched = false;
            var tryCount = 0;
            while (!fetched && tryCount < 10)
            {
                try
                {
                    tryCount++;
                    chatters = await this._twitchApi.Undocumented.GetChattersAsync(this._userInfo.Login);
                    fetched = true;
                }
                catch (Exception e)
                {
                    TwitchPlugin.PluginLog.Warning(e, "TwitchPlugin.FetchViewersAsync error: " + e.Message);
                }
            }

            lock (this._viewersLock)
            {
                foreach (var chatter in chatters)
                {
                    if (chatter.Username != this._userInfo.Login)
                    {
                        this.Chatters.Add(chatter.Username);
                    }
                }
            }

            this.ChattersChanged?.Invoke(this, this.Chatters);
        }

        internal void InitViewersUpdater()
        {
            TwitchPlugin.PluginLog.Info("TwitchWrapper InitViewersUpdater");
            this.StopViewersUpdater();
            this._viewerUpdaterCancellationTokenSource = new CancellationTokenSource();
            Task.Run(this.StartUpdateViewersAsync, this._viewerUpdaterCancellationTokenSource.Token);
        }

        internal void StopViewersUpdater()
        {
            TwitchPlugin.PluginLog.Info("TwitchWrapper StopViewersUpdater");

            try
            {
                this._viewerUpdaterCancellationTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                TwitchPlugin.PluginLog.Error(ex, "Error stopping viewers updater");
            }
        }

        private async Task StartUpdateViewersAsync()
        {
            TwitchPlugin.PluginLog.Info("TwitchWrapper StartUpdateViewersAsync");

            try
            {
                while (!this._viewerUpdaterCancellationTokenSource.IsCancellationRequested)
                {
                    await this.UpdateViewersAsync();
                    await Task.Delay(10000);
                }
            }
            catch (ThreadAbortException ex)
            {
                TwitchPlugin.PluginLog.Error(ex,"\nTwitch UpdateViewers stopped");
            }
        }

        private async Task UpdateViewersAsync()
        {
            try
            {
                if (!String.IsNullOrEmpty(this._twitchApi.Settings.AccessToken) && this._twitchClient.IsConnected)
                {
                    this.CurrentViewersCount = await this.GetViewersAsync(this._userInfo.Login);
                    this.ViewersChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex) when (ex is TokenExpiredException || ex is BadScopeException)
            {
                this.StopViewersUpdater();
                this.AccessTokenExpired?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                TwitchPlugin.PluginLog.Error(ex,$"TwitchWrapper UpdateViewersAsync error: {ex.Message}");
            }
        }

        private async Task<Int32> GetViewersAsync(String userLogin)
        {
            var stream = await this._twitchApi.Helix.Streams
                .GetStreamsAsync(first: 1, userLogins: new List<string> { userLogin }) ?? null;
            var currentStream = stream?.Streams?.FirstOrDefault();
            return currentStream?.ViewerCount ?? 0;
        }

        private static UserInfo GetUserInfo(String accessToken)
        {
            var tokenInfo = TwitchHelpers.GetTokenInfo(accessToken);
            var userInfo = new UserInfo(tokenInfo.UserId, tokenInfo.Login);
            return userInfo;
        }
    }
}

#if false
    case "ToggleSlowChat":
                    TwitchPlugin.TwitchWrapper.SendMessage(TwitchPlugin.TwitchWrapper.SlowMode != 0
                        ? ".slowoff"
                        : ".slow 30");
                    break;
                case "ToggleEmotesOnly":
                    TwitchPlugin.TwitchWrapper.SendMessage(TwitchPlugin.TwitchWrapper.IsEmoteOnly
                        ? ".emoteonlyoff"
                        : ".emoteonly");
                    break;
                case "ToggleFollowersOnly":
                    TwitchPlugin.TwitchWrapper.SendMessage(TwitchPlugin.TwitchWrapper.FollowersOnly != TimeSpan.Zero
                        ? ".followersoff"
                        : ".followers 10m");
                    break;
                case "ToggleSubsOnly":
                    TwitchPlugin.TwitchWrapper.SendMessage(TwitchPlugin.TwitchWrapper.IsSubOnly
                        ? ".subscribersoff"
                        : ".subscribers");
                    break;
#endif