namespace Loupedeck.TwitchPlugin
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using TwitchLib.Api;
    using TwitchLib.Api.Core;
    using TwitchLib.Api.Core.Exceptions;
    using TwitchLib.Client;
    using TwitchLib.Client.Events;
    using TwitchLib.Client.Models;
    using TwitchLib.Communication.Events;

    /*
     NOTES ON CONNECTION LOGIC
        Connecting Twitch Client from: 

        TwitchPlugin.OnOnlineContentReceived  (cached token retreived)
        TwitchPlugin.OnTokenReceived          (authenticated)
        TwitchPlugin.ConnectionMonitorTimeout                

            -> Successful OnTwitchClientConnected

        Disconnecting from:
        
        OnTwitchAccountOnLogoutRequested (user disconnectes)
        Connect (if connected)
        IncorrectLogin 

            -> Successful OnTwitchClientDisconnected
     
     */

    public partial class TwitchWrapper : IDisposable
    {
        public TwitchAPI twitchApi { get; private set; }
        private readonly TwitchClient _twitchClient;
        private UserInfo _userInfo;

        private ChannelState _channelState;

        private TwitchPluginLoggerFactory _loggerFactory = new TwitchPluginLoggerFactory();

        public EventHandler AccessTokenExpired { get; set; }
        public EventHandler<String> Connected { get; set; }
        public EventHandler Disconnected { get; set; }
        public EventHandler<String> Reconnected { get; set; }
        public EventHandler<String> ConnectionError { get; set; }
        public EventHandler<Exception> Error { get; set; }
        public EventHandler<(String, Exception)> IncorrectLogin { get; set; }
        public Boolean IsConnected => this._twitchClient?.IsConnected == true;

        public TwitchWrapper()
        {
            this.twitchApi = new TwitchAPI(loggerFactory: this._loggerFactory, settings: new ApiSettings
            {
                ClientId = null,/* These will be set when we get config  data */
                Secret = null,
                SkipAutoServerTokenGeneration = true,
                SkipDynamicScopeValidation = true
            });

            this._twitchClient = new TwitchClient(logger: this._loggerFactory.CreateLogger<TwitchLib.Client.TwitchClient>());

            this._twitchClient.OnConnected += this.OnTwitchClientConnected;
            this._twitchClient.OnDisconnected += this.OnTwitchClientDisconnected;

            this._twitchClient.OnJoinedChannel += this.OnJoinedChannel;
            this._twitchClient.OnChannelStateChanged += this.OnChannelStateChanged;
            /* we are not using these yet:  
             * this._twitchClient.OnUserJoined += this.OnUserJoined;
             * this._twitchClient.OnUserLeft += this.OnUserLeft;
             */
            this._twitchClient.OnReconnected += this.OnReconnected;
            this._twitchClient.OnIncorrectLogin += this.OnIncorrectLogin;
            this._twitchClient.OnConnectionError += this.OnConnectionError;
            this._twitchClient.OnError += this.OnError;
 
        }

        public void SetClientCredentials(String clientID, String clientSecret)
        {
            this.twitchApi.Settings.ClientId = clientID;
            this.twitchApi.Settings.Secret = clientSecret; 
        }



        public void Connect(String accessToken)
        {
            TwitchPlugin.PluginLog.Info("TwitchWrapper Connect");

            try
            {

                this.twitchApi.Settings.AccessToken = accessToken;
                if (this._twitchClient?.IsConnected == true)
                {
                    TwitchPlugin.PluginLog.Info("Already connected. Disconnecting");

                    //this.StopViewersUpdater();
                    //FIXME: Check if we need to suspend OnDisconnected of twitchClient
                    this._twitchClient.Disconnect();
                    //this._twitchClient.Dispose();
                }

                
                //FIXME: WHY WE ALWAYS NEED TO GO ASK TWITCH FOR THAT???
                var tokenInfo = AuthenticationServer.GetTokenInfo(accessToken);
                this._userInfo = new UserInfo(tokenInfo.UserId, tokenInfo.Login);



                this._twitchClient.Initialize(
                    new ConnectionCredentials(this._userInfo.Login, accessToken),
                    this._userInfo.Login);

                //await this.FetchChattersAsync();

                if(!this._twitchClient.Connect())
                {
                    TwitchPlugin.PluginLog.Error("Error executing twitchClient.Connect");
                } 
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
                if (this.twitchApi != null)
                {
                    this.twitchApi.Settings.AccessToken = null;
                }
               
                if (this._twitchClient == null)
                {
                    return;
                }

                this.StopViewersUpdater();

                this._twitchClient.Disconnect();
                //this._twitchClient.Dispose()
                this.CurrentViewersCount = 0;
                
                //this.Chatters.Clear();
                //this.ChattersChanged?.Invoke(this, this.Chatters);

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
                //This IF statement is there so that we ensure we are on the channel before sending a message 
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

        public void Dispose()
        {
            this.StopViewersUpdater();
            //this._twitchClient?.Dispose();
            this._viewerUpdaterCancellationTokenSource?.Dispose();
            this._twitchClient.OnConnected -= this.OnTwitchClientConnected;
            this._twitchClient.OnDisconnected -= this.OnTwitchClientDisconnected;

            this._twitchClient.OnJoinedChannel -= this.OnJoinedChannel;
            this._twitchClient.OnChannelStateChanged -= this.OnChannelStateChanged;

            //this._twitchClient.OnUserJoined -= this.OnUserJoined;
            //this._twitchClient.OnUserLeft -= this.OnUserLeft;

            this._twitchClient.OnReconnected -= this.OnReconnected;
            
            this._twitchClient.OnIncorrectLogin -= this.OnIncorrectLogin;
            this._twitchClient.OnConnectionError -= this.OnConnectionError;
            this._twitchClient.OnError -= this.OnError;
        }

        

        private void OnConnectionError(Object sender, OnConnectionErrorArgs e)
        {
            this.StopViewersUpdater();
            this.ConnectionError?.Invoke(sender, e.Error.Message);
        }

        private void OnTwitchClientDisconnected(Object sender, OnDisconnectedEventArgs e)
        {
            this.StopViewersUpdater();
            this.CurrentViewersCount = 0;
            //this.Chatters.Clear();
            //this.ChattersChanged?.Invoke(this, this.Chatters);
            this.ViewersChanged?.Invoke(this, EventArgs.Empty);
            this.Disconnected?.Invoke(sender, EventArgs.Empty);
        }

        private void OnIncorrectLogin(Object sender, OnIncorrectLoginArgs e)
        {
            this.IncorrectLogin?.Invoke(sender, (e.Exception.Username, e.Exception));
            this._twitchClient.Disconnect();
        }

        private void OnTwitchClientConnected(Object sender, OnConnectedArgs e)
        {
            // need to ensure that we only subscribe once
            this.UpdateViewersAsync().ConfigureAwait(false);
            this.InitViewersUpdater();
            this.Connected?.Invoke(sender, e.BotUsername);
        }

        private void OnReconnected(Object sender, OnReconnectedEventArgs e)
        {
            // need to ensure that we only subscribe once
            this.InitViewersUpdater();
            this.Reconnected?.Invoke(sender, this._userInfo.Login);
        }

        private void OnError(Object sender, OnErrorEventArgs e)
        {
            this.Error?.Invoke(sender, e.Exception);
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