﻿namespace Loupedeck.TwitchPlugin
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TwitchLib.Api;
    using TwitchLib.Api.Core;
    using TwitchLib.Api.Core.Exceptions;
    using TwitchLib.Client;
    using TwitchLib.Client.Events;
    using TwitchLib.Client.Interfaces;
    using TwitchLib.Communication.Clients;
    using TwitchLib.Communication.Events;
    using TwitchLib.Communication.Interfaces;
    using TwitchLib.Communication.Models;

    /*
     NOTES ON CONNECTION LOGIC
        Connecting Twitch Client from: 

        TwitchPlugin.OnOnlineContentReceived  (cached token retreived)
        TwitchPlugin.OnTokenReceived          (authenticated)

            -> Successful OnTwitchClientConnected

        Disconnecting from:
        
        OnTwitchAccountOnLogoutRequested (user disconnectes)
        Connect (if connected)
        IncorrectLogin 

            -> Successful OnTwitchClientDisconnected
     
     */

    public partial class TwitchProxy : IDisposable
    {
        public TwitchAPI twitchApi { get; private set; }

        public EventHandler<Exception> Error { get; set; }

        private UserInfo _userInfo;

        private readonly TwitchPluginLoggerFactory _loggerFactory = new TwitchPluginLoggerFactory();
        
        private IClient _webSocketClient;
        private TwitchClient _twitchClient;

        private void InitializeTwitchClient()
        {
            //Using workaround for continuous reconnection from here:
            // https://github.com/TwitchLib/TwitchLib.Client/issues/206#issuecomment-1407447681
            IClientOptions options = new ClientOptions();

            this._webSocketClient = new WebSocketClient(options);
            this._twitchClient = new TwitchClient(client: this._webSocketClient, 
                logger: this._loggerFactory.CreateLogger<TwitchLib.Client.TwitchClient>());

            this._twitchClient.OnConnected += this.OnTwitchClientConnected;
            this._twitchClient.OnDisconnected += this.OnTwitchClientDisconnected;

            this._twitchClient.OnUnaccountedFor += this.OnUnaccountedFor;

            this._twitchClient.OnConnected += this.StartViewersUpdateTimer;
            this._twitchClient.OnDisconnected += this.StopViewersUpdateTimer;

            this._twitchClient.OnJoinedChannel += this.OnJoinedChannel;
            this._twitchClient.OnChannelStateChanged += this.OnChannelStateChanged;

            /* we are not using these yet:  
             * this._twitchClient.OnUserJoined += this.OnUserJoined;
             * this._twitchClient.OnUserLeft += this.OnUserLeft;
             */
            this._twitchClient.OnReconnected += this.OnTwtchClientReconnected;
            this._twitchClient.OnIncorrectLogin += this.OnTwitchIncorrectLogin;
            this._twitchClient.OnConnectionError += this.OnTwitchConnectionError;
            this._twitchClient.OnError += this.OnError;
        }
        private void DisposeTwitchClient()
        {
            this._refreshTokenTimer.Enabled = false;

            this.StopViewersUpdateTimer(this, null);

            if (this._twitchClient == null)
            {
                return;
            }

            this._twitchClient.OnConnected -= this.OnTwitchClientConnected;
            this._twitchClient.OnDisconnected -= this.OnTwitchClientDisconnected;
            this._twitchClient.OnUnaccountedFor -= this.OnUnaccountedFor;

            this._twitchClient.OnConnected -= this.StartViewersUpdateTimer;
            this._twitchClient.OnDisconnected -= this.StopViewersUpdateTimer;


            this._twitchClient.OnJoinedChannel -= this.OnJoinedChannel;
            this._twitchClient.OnChannelStateChanged -= this.OnChannelStateChanged;

            //this._twitchClient.OnUserJoined -= this.OnUserJoined;
            //this._twitchClient.OnUserLeft -= this.OnUserLeft;

            this._twitchClient.OnReconnected -= this.OnTwtchClientReconnected;

            this._twitchClient.OnIncorrectLogin -= this.OnTwitchIncorrectLogin;
            this._twitchClient.OnConnectionError -= this.OnTwitchConnectionError;
            this._twitchClient.OnError -= this.OnError;

            this._webSocketClient.Close();
            this._twitchClient.Disconnect();
            this._webSocketClient.Dispose();
            this._webSocketClient = null;
            this._twitchClient = null;
        }

        public TwitchProxy()
        {
            this.twitchApi = new TwitchAPI(loggerFactory: this._loggerFactory, settings: new ApiSettings
            {
                ClientId = null,/* These will be set when we get config  data */
                Secret = null,
                SkipAutoServerTokenGeneration = true,
                SkipDynamicScopeValidation = true
            });

            this.OnTwitchAccessTokenExpired += this.OnAccessTokenExpired;

            this._refreshTokenTimer = new System.Timers.Timer();
            this._refreshTokenTimer.AutoReset = false;
            this._refreshTokenTimer.Elapsed += (e, s) => this.OnRefreshTokenTimerTick(null, null);
            this._refreshTokenTimer.Enabled = false;

            this._viewersUpdatetimer = new System.Timers.Timer();
            this._viewersUpdatetimer.AutoReset = true;
            this._viewersUpdatetimer.Interval = 10000;
            this._viewersUpdatetimer.Elapsed += (e, s) => this.OnViewersUpdateTimerTick(null, null);

            this.InitializeTwitchClient();

            this._authServer = new AuthenticationServer();
            this._authServer.OnTokenReceived += this.OnAccessTokenReceived;
            this._authServer.OnTokenError += this.OnAccessTokenError;

            this.SetChannelFlags(null);
        }

        public void SendMessage(String message)
        {
            try
            {
                this.EnsureOnOwnChannel(() =>
                {
                    this._twitchClient.SendMessage(this._userInfo.Login, message);
                });
            }
            catch (TokenExpiredException)
            {
                this.OnTwitchAccessTokenExpired?.BeginInvoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            this.OnTwitchAccessTokenExpired -= this.OnAccessTokenExpired;
            this._authServer.OnTokenReceived -= this.OnAccessTokenReceived;
            this._authServer.OnTokenError -= this.OnAccessTokenError;
            this.DisposeTwitchClient();
        }

        private void OnError(Object sender, OnErrorEventArgs e) => this.Error?.Invoke(sender, e.Exception);

#if DEBUG
        public Dictionary<String,String> GetDebugCommands()
        {
            String[] strings = new[] 
                { 
                    "AccessTokenExpired",
                    "OnTwitchClientDisconnected",
                    "OnTwitchConnectionError",
                    "Disconnect",
                    "Connect"
                };

            return strings.ToDictionary(key => key, value => value);

        }

        public void RunDebugCommand(String parameter)
        {
            TwitchPlugin.PluginLog.Info($"Debug run with {parameter}");
            switch (parameter)
            {
                case  "AccessTokenExpired":
                    this.OnTwitchAccessTokenExpired.BeginInvoke(this, EventArgs.Empty);
                    break;
                case "OnTwitchConnectionError":
                {
                    var e = new OnConnectionErrorArgs();
                    e.Error.Message = "Syntetic!";
                    this.OnTwitchConnectionError(this, e);
                    break;
                }
                 
                case "OnTwitchClientDisconnected":
                {
                    var e = new OnDisconnectedEventArgs();
                    this.OnTwitchClientDisconnected(this, e);
                    break;
                }
                case "Disconnect":
                {
                    this._twitchClient.Disconnect();
                    break;
                }
                case "Connect":
                {
                    //https://github.com/TwitchLib/TwitchLib/issues/1063
                    this.DoConnect();
                    break;
                }

            }
        }
#endif

    }
}

#if false
    case "ToggleSlowChat":
                    TwitchPlugin.TwitchPlugin.SendMessage(TwitchPlugin.TwitchPlugin.SlowMode != 0
                        ? ".slowoff"
                        : ".slow 30");
                    break;
                case "ToggleEmotesOnly":
                    TwitchPlugin.TwitchPlugin.SendMessage(TwitchPlugin.TwitchPlugin.IsEmoteOnly
                        ? ".emoteonlyoff"
                        : ".emoteonly");
                    break;
                case "ToggleFollowersOnly":
                    TwitchPlugin.TwitchPlugin.SendMessage(TwitchPlugin.TwitchPlugin.FollowersOnly != TimeSpan.Zero
                        ? ".followersoff"
                        : ".followers 10m");
                    break;
                case "ToggleSubsOnly":
                    TwitchPlugin.TwitchPlugin.SendMessage(TwitchPlugin.TwitchPlugin.IsSubOnly
                        ? ".subscribersoff"
                        : ".subscribers");
                    break;
#endif