namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    public class TwitchPlugin : Plugin
    {
        public static PluginLogFile PluginLog { get; private set; } = null;

        private const String ConfigFileName = "twitch-v2.json";

        private readonly PluginPreferenceAccount _twitchAccount;

        private System.Timers.Timer _connectionCheckTimer;
        private IAuthenticationServer _authenticationServer;
        private Boolean _refreshAccessTokenRequestBlocked;

        public override Boolean UsesApplicationApiOnly => true;
        public override Boolean HasNoApplication => true;
        internal static TwitchWrapper Proxy { get; private set; }

        public class PluginLogFileY: PluginLogFile
        {
            public PluginLogFileY(string x) : base(x) { }
            public void Error(String s, Exception ex) => this.Error(s + " exception:"+ ex.ToString());
            public void Warning(String s, Exception ex) => this.Warning(s + " exception:" + ex.ToString());
        }

    public TwitchPlugin()
        {
            if(TwitchPlugin.PluginLog == null) 
                TwitchPlugin.PluginLog = this.Log; 

            this._twitchAccount = new PluginPreferenceAccount("twitch-account")
            {
                DisplayName = "Twitch Account",
                IsRequired = true,
                LoginUrlTitle = "Sign in to Twitch",
                LogoutUrlTitle = "Sign out from Twitch"
            };

            this.PluginPreferences.Add(this._twitchAccount);
            TwitchPlugin.Proxy = new TwitchWrapper();
        }

        public override void Load()
        {
            base.Load();
            this.LoadPluginIcons();

            TwitchPlugin.Proxy.Connected += this.OnConnected;
            TwitchPlugin.Proxy.Disconnected += this.OnDisconnected;
            TwitchPlugin.Proxy.Reconnected += this.OnReconnected;
            TwitchPlugin.Proxy.ConnectionError += this.OnConnectionError;
            TwitchPlugin.Proxy.Error += this.OnError;
            TwitchPlugin.Proxy.IncorrectLogin += this.OnIncorrectLogin;
            TwitchPlugin.Proxy.ChannelStatusChanged += this.OnChannelStateChanged;
            TwitchPlugin.Proxy.ViewersChanged += this.OnViewersChanged;
            TwitchPlugin.Proxy.AccessTokenExpired += this.OnAccessTokenExpired;
            this._connectionCheckTimer = new System.Timers.Timer(5000);
            this._connectionCheckTimer.Elapsed += this.ConnectionCheckTimerOnElapsed;
            this._authenticationServer = new AuthenticationServer();
            this._authenticationServer.TokenReceived += this.OnTokenReceived;
            this._twitchAccount.LoginRequested += this.OnTwitchAccountOnLoginRequested;
            this._twitchAccount.LogoutRequested += this.OnTwitchAccountOnLogoutRequested;
            this.ServiceEvents.OnlineFileContentReceived += this.OnOnlineFileContentReceived;
            this.ServiceEvents.GetOnlineFileContent(ConfigFileName);
        }

        public override void Unload()
        {
            this._connectionCheckTimer.Stop();
            this._connectionCheckTimer.Elapsed -= this.ConnectionCheckTimerOnElapsed;
            TwitchPlugin.Proxy.Connected -= this.OnConnected;
            TwitchPlugin.Proxy.Disconnected -= this.OnDisconnected;
            TwitchPlugin.Proxy.Reconnected -= this.OnReconnected;
            TwitchPlugin.Proxy.ConnectionError -= this.OnConnectionError;
            TwitchPlugin.Proxy.Error -= this.OnError;
            TwitchPlugin.Proxy.IncorrectLogin -= this.OnIncorrectLogin;
            TwitchPlugin.Proxy.ChannelStatusChanged -= this.OnChannelStateChanged;
            TwitchPlugin.Proxy.ViewersChanged -= this.OnViewersChanged;
            TwitchPlugin.Proxy.AccessTokenExpired -= this.OnAccessTokenExpired;
            this._authenticationServer.TokenReceived -= this.OnTokenReceived;
            this._twitchAccount.LoginRequested -= this.OnTwitchAccountOnLoginRequested;
            this._twitchAccount.LogoutRequested -= this.OnTwitchAccountOnLogoutRequested;
            this.ServiceEvents.OnlineFileContentReceived -= this.OnOnlineFileContentReceived;
            TwitchPlugin.Proxy.Dispose();
            this._authenticationServer.Dispose();
        }

        public override void RunCommand(String commandName, String parameter)
        {
            if (!TwitchPlugin.Proxy.IsConnected)
            {
                TwitchPlugin.PluginLog.Warning("Skip command: not connected to twitch client.");
                return;
            }

            try
            {
                this.ProcessCommand(commandName, parameter);
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e,"RunCommand: unhandled exception.");
            }
        }

        public override void ApplyAdjustment(String adjustmentName, String parameter, Int32 diff)
        {

        }

        public override PluginActionParameter[] GetActionParameterValues(String actionName)
        {
            PluginActionParameter[] GetListParameters(String displayName, Dictionary<String, String> values)
            {
                var parameters = new List<PluginActionParameter>();
                parameters.AddRange(values.Select(value =>
                    new PluginActionParameter(value.Key, $"{this.Localization.GetString(displayName)} {this.Localization.GetString(value.Value)}", "Twitch")));
                return parameters.ToArray();
            }

            switch (actionName)
            {
                case "ToggleSlowChatList":
                    return GetListParameters("Slow Mode", DisplayNameMaps.SlowModeNamesMap);
                case "RunCommercialList":
                    return GetListParameters("Run Commercial", DisplayNameMaps.RunCommercialNamesMap);
                case "ToggleFollowersOnlyList":
                    return GetListParameters("Followers-Only", DisplayNameMaps.FollowersOnlyNamesMap);
                default:
                    return Array.Empty<PluginActionParameter>();
            }
        }

        protected override bool TryGetActionImage(string actionName, string actionParameter, PluginImageSize imageSize,
            out BitmapImage bitmap)
        {
            BitmapImage GetViewersBitmapWithText(String imageName, String buttonText, Int32 fontSize = 15, BitmapColor? color = null)
            {
                using (var bitmapBuilder = new BitmapBuilder(imageSize))
                {
                    bitmapBuilder.DrawLibraryImage(imageName, imageSize);

                    if (!String.IsNullOrEmpty(buttonText))
                    {
                        //Rectangle rectangle = new Rectangle();
                        //switch (imageSize)
                        //{
                        //    case PluginImageSize.Width60:
                        //        //todo
                        //        break;
                        //    case PluginImageSize.Width90:
                        //        rectangle = new Rectangle(10, 30, 60, 60);
                        //        break;
                        //}
                        bitmapBuilder.DrawText(buttonText, 10, 30, 60, 60, color: color, fontSize: fontSize); //, fontSize: 9);
                    }

                    return bitmapBuilder.ToImage();
                }
            }

            switch (actionName)
            {
                case "ViewerCount":
                    bitmap = GetViewersBitmapWithText("Twitch/TwitchViewers.png",
                        $"{TwitchPlugin.Proxy.CurrentViewersCount}");
                    return true;
                case "ToggleSlowChatList" when !String.IsNullOrEmpty(actionParameter):
                    bitmap = GetViewersBitmapWithText(TwitchPlugin.Proxy.SlowMode == Int32.Parse(actionParameter)
                            ? "Twitch/TwitchSlowChat.png"
                            : "Twitch/TwitchSlowChatToggle.png",
                        this.Localization.GetString(DisplayNameMaps.SlowModeNamesMap[actionParameter]), 11);
                    return true;
                case "ToggleFollowersOnlyList" when !String.IsNullOrEmpty(actionParameter):
                    var isFollowersOnly = TwitchPlugin.Proxy.FollowersOnly == ParseTimeSpan(actionParameter);
                    bitmap = GetViewersBitmapWithText(isFollowersOnly
                            ? "Twitch/TwitchFollowerChat.png"
                            : "Twitch/TwitchFollowerChatToggle.png",
                        this.Localization.GetString(DisplayNameMaps.FollowersOnlyNamesMap[actionParameter]), 11,
                        isFollowersOnly
                            ? BitmapColor.Black
                            : BitmapColor.White);
                    return true;
                case "RunCommercialList" when !String.IsNullOrEmpty(actionParameter):
                    bitmap = GetViewersBitmapWithText("Twitch/TwitchAd1.png",
                        this.Localization.GetString(DisplayNameMaps.RunCommercialNamesMap[actionParameter]), 11);
                    return true;
            }

            return base.TryGetActionImage(actionName, actionParameter, imageSize, out bitmap);
        }

        protected override Boolean TryGetActionLibraryImage(String actionName, String actionParameter,
            out String imageFileName, out String imageLibraryName)
        {
            imageLibraryName = null;
            switch (actionName)
            {
                case "ToggleSubsOnly":
                    imageFileName = TwitchPlugin.Proxy.IsSubOnly
                        ? "Twitch/TwitchSubChat2.png"
                        : "Twitch/TwitchSubChat.png";
                    return true;
                case "ToggleFollowersOnly":
                    imageFileName = TwitchPlugin.Proxy.FollowersOnly == TimeSpan.Zero
                        ? "Twitch/TwitchFollowerChatToggle.png"
                        : "Twitch/TwitchFollowerChat.png";
                    return true;
                case "ToggleEmotesOnly":
                    imageFileName = TwitchPlugin.Proxy.IsEmoteOnly
                        ? "Twitch/TwitchEmoteChatToggle.png"
                        : "Twitch/TwitchEmoteChat.png";
                    return true;
                case "ToggleSlowChat":
                    imageFileName = TwitchPlugin.Proxy.SlowMode == 0
                        ? "Twitch/TwitchSlowChatToggle.png"
                        : "Twitch/TwitchSlowChat.png";
                    return true;
                default:
                    imageFileName = null;
                    return false;
            }
        }

        private void ProcessCommand(String commandName, String parameter)
        {
            switch (commandName)
            {
                case "SendChatMessage":
                    TwitchPlugin.Proxy.SendMessage(parameter);
                    break;
                case "ToggleSlowChat":
                    TwitchPlugin.Proxy.SendMessage(TwitchPlugin.Proxy.IsSlowMode
                        ? ".slowoff"
                        : ".slow 30");
                    break;
                case "ToggleEmotesOnly":
                    TwitchPlugin.Proxy.SendMessage(TwitchPlugin.Proxy.IsEmoteOnly
                        ? ".emoteonlyoff"
                        : ".emoteonly");
                    break;
                case "ToggleFollowersOnly":
                    TwitchPlugin.Proxy.SendMessage(TwitchPlugin.Proxy.IsFollowersOnly
                        ? ".followersoff"
                        : ".followers 10m");
                    break;
                case "ToggleSubsOnly":
                    TwitchPlugin.Proxy.SendMessage(TwitchPlugin.Proxy.IsSubOnly
                        ? ".subscribersoff"
                        : ".subscribers");
                    break;
                case "ClearChat":
                    TwitchPlugin.Proxy.SendMessage(".clear");
                    break;
                case "RunCommercial":
                    TwitchPlugin.Proxy.SendMessage(".commercial");
                    break;
                case "CreateMarker":
                    TwitchPlugin.Proxy.CreateMarkerCommandAsync().ConfigureAwait(false);
                    break;
                case "CreateClip":
                    TwitchPlugin.Proxy.CreateClipCommandAsync().ConfigureAwait(false);
                    break;
                case "ToggleSlowChatList":
                    TwitchPlugin.Proxy.SendMessage(TwitchPlugin.Proxy.SlowMode == Int32.Parse(parameter)
                        ? ".slowoff"
                        : $".slow {parameter}");
                    break;
                case "ToggleFollowersOnlyList":
                    TwitchPlugin.Proxy.SendMessage(TwitchPlugin.Proxy.FollowersOnly == ParseTimeSpan(parameter)
                        ? ".followersoff"
                        : $".followers {parameter}");
                    break;
                case "RunCommercialList":
                    TwitchPlugin.Proxy.SendMessage($".commercial {parameter}");
                    break;
                case "ResetSettings":
                    this.ResetSettings();
                    break;
            }
        }

        private void OnTwitchAccountOnLoginRequested(Object sender, EventArgs e)
        {
            TwitchPlugin.PluginLog.Info("TwitchPlugin OnTwitchAccountOnLoginRequested");
            this._authenticationServer.Authenticate();
        }

        private void OnTwitchAccountOnLogoutRequested(Object sender, EventArgs e)
        {
            TwitchPlugin.PluginLog.Info("TwitchPlugin OnTwitchAccountOnLogoutRequested");
            this._twitchAccount.AccessToken = null;
            this._twitchAccount.RefreshToken = null;
            TwitchPlugin.Proxy.Disconnect();
            this._twitchAccount.ReportLogout();
        }

        private void OnError(Object sender, Exception e)
        {
            TwitchPlugin.PluginLog.Warning(e,$"TwitchAPI OnError received: {e.Message}");
            var webSocketException = e as System.Net.WebSockets.WebSocketException;
            if (webSocketException?.ErrorCode == null && !(e is WebException))
            {
                return;
            }

            switch (webSocketException?.ErrorCode)
            {
                case 258:
                    this._connectionCheckTimer.Stop();
                    this.RequestRefreshAccessToken(this._twitchAccount.RefreshToken);
                    break;
            }
        }

        private void OnConnectionError(Object sender, String message)
        {
            TwitchPlugin.PluginLog.Info($"Connection Error: {message}");
            this._connectionCheckTimer.Stop();
            if (!message.Equals("Fatal network error. Network services fail to shut down."))
            {
                this.RequestRefreshAccessToken(this._twitchAccount.RefreshToken);
            }
        }

        private void OnDisconnected(Object sender, EventArgs e)
        {
            TwitchPlugin.PluginLog.Info("Disconnected.");
            this._connectionCheckTimer.Stop();
            this._twitchAccount.ReportLogout();
        }

        private void OnIncorrectLogin(Object sender, (String, Exception) e)
        {
            var (_, ex) = e;
            TwitchPlugin.PluginLog.Warning(ex,$"Incorrect Login: {ex.Message}");
            this._twitchAccount.ReportLogout();
        }

        private void OnConnected(Object sender, String username)
        {
            TwitchPlugin.PluginLog.Info($"Connected to twitch client: {username}");
            this._twitchAccount.ReportLogin(username, this._twitchAccount.AccessToken,
                this._twitchAccount.RefreshToken);
            this._connectionCheckTimer.Start();
        }

        private void OnReconnected(Object sender, String username)
        {
            TwitchPlugin.PluginLog.Info("Reconnected.");
            if (!TwitchPlugin.Proxy.IsConnected ||
                !this._authenticationServer.IsTokenValid(this._twitchAccount.AccessToken))
            {
                return;
            }

            this._twitchAccount.ReportLogin(username, this._twitchAccount.AccessToken,
                this._twitchAccount.RefreshToken);
            this._connectionCheckTimer.Start();
        }

        private void OnChannelStateChanged(Object sender, EventArgs e)
        {
            this.OnActionImageChanged("ToggleSubsOnly", String.Empty);
            this.OnActionImageChanged("ToggleFollowersOnly", String.Empty);
            this.OnActionImageChanged("ToggleEmotesOnly", String.Empty);
            this.OnActionImageChanged("ToggleSlowChat", String.Empty);
            this.OnActionImageChanged("ToggleSlowChatList", String.Empty);
            this.OnActionImageChanged("ToggleFollowersOnlyList", String.Empty);
        }

        private void OnViewersChanged(Object sender, EventArgs e)
        {
            this.OnActionImageChanged("ViewerCount", null);
        }

        private void OnAccessTokenExpired(Object sender, EventArgs e)
        {
            TwitchPlugin.PluginLog.Info("TwitchPlugin OnAccessTokenExpired");
            this.RequestRefreshAccessToken(this._twitchAccount.RefreshToken);
        }

        private void RequestRefreshAccessToken(String refreshToken)
        {
            TwitchPlugin.PluginLog.Info("TwitchPlugin RequestRefreshAccessToken");
            if (this._refreshAccessTokenRequestBlocked)
            {
                return;
            }

            this._refreshAccessTokenRequestBlocked = true;

            try
            {
                TwitchPlugin.PluginLog.Warning("Access token has expired.");
                this._authenticationServer.RefreshAccessToken(refreshToken);
            }
            catch (Exception ex)
            {
                TwitchPlugin.PluginLog.Warning(ex, "Twitch: Failed to refresh token.");
                this._twitchAccount.ReportLogout();
            }
            finally
            {
                this._refreshAccessTokenRequestBlocked = false;
            }
        }

        private void ResetSettings()
        {
            var messages = new List<String>();

            if (TwitchPlugin.Proxy.IsSlowMode)
            {
                messages.Add(".slowoff");
            }

            if (TwitchPlugin.Proxy.IsEmoteOnly)
            {
                messages.Add(".emoteonlyoff");
            }

            if (TwitchPlugin.Proxy.IsFollowersOnly)
            {
                messages.Add(".followersoff");
            }

            if (TwitchPlugin.Proxy.IsSubOnly)
            {
                messages.Add(".subscribersoff");
            }

            foreach (var message in messages)
            {
                TwitchPlugin.Proxy.SendMessage(message);
            }
        }

        private void OnOnlineFileContentReceived(Object sender, OnlineFileContentReceivedEventArgs e)
        {
            TwitchPlugin.PluginLog.Info("TwitchPlugin OnOnlineFileContentReceived");
            if (!e.FileName.EqualsNoCase(ConfigFileName))
            {
                TwitchPlugin.PluginLog.Info("TwitchPlugin OnOnlineFileContentReceived: file name not match");
                return;
            }

            var json = System.Text.Encoding.UTF8.GetString(e.GetFileContent() ?? Array.Empty<Byte>());
            if (!JsonHelpers.TryDeserializeAnyObject<TwitchPluginConfig>(json, out var pluginConfig) || pluginConfig == null)
            {
                TwitchPlugin.PluginLog.Warning(
                    "TwitchPlugin OnOnlineFileContentReceived: couldn't deserialize TwitchPluginConfig json, reading plugin config file from cache");
                if (!this.TryGetPluginSetting(ConfigFileName, out json))
                {
                    TwitchPlugin.PluginLog.Error("TwitchPlugin OnOnlineFileContentReceived: couldn't read config file from cache");
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "No connection to Loupedeck Server.", null,
                        null);
                    return;
                }

                pluginConfig = JsonHelpers.DeserializeObject<TwitchPluginConfig>(json);
            }
            else
            {
                this.SetPluginSetting(ConfigFileName, json);
            }

            TwitchPlugin.Proxy.Initialize(pluginConfig.ClientId, pluginConfig.ClientSecret);
            this._authenticationServer.Start(pluginConfig);
            if (String.IsNullOrEmpty(this._twitchAccount.AccessToken) ||
                String.IsNullOrEmpty(this._twitchAccount.RefreshToken))
            {
                TwitchPlugin.PluginLog.Info("TwitchPlugin OnOnlineFileContentReceived: token not cached");
                this._twitchAccount.ReportLogout();
                return;
            }

            if (!this._authenticationServer.IsTokenValid(this._twitchAccount.AccessToken))
            {
                TwitchPlugin.PluginLog.Info("TwitchPlugin OnOnlineFileContentReceived: cached toke expired");
                this.RequestRefreshAccessToken(this._twitchAccount.RefreshToken);
                return;
            }

            try
            {
                TwitchPlugin.Proxy.Connect(this._twitchAccount.AccessToken).Wait();
            }
            catch (Exception)
            {
                this._twitchAccount.ReportLogout();
            }
        }

        private void OnTokenReceived(Object sender, Token token)
        {
            try
            {
                if (token.AccessToken == null || token.RefreshToken == null)
                {
                    TwitchPlugin.PluginLog.Warning("Twitch OnTokenReceived: Empty access token.");
                    return;
                }

                this._twitchAccount.AccessToken = token.AccessToken;
                this._twitchAccount.RefreshToken = token.RefreshToken;
                TwitchPlugin.Proxy.Connect(token.AccessToken).Wait();
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e,$"Twitch OnTokenReceived error: {e.Message}");
                this._twitchAccount.ReportLogout();
            }
        }

        private void LoadPluginIcons()
        {
            this.Info.Icon16x16 = EmbeddedResources.ReadImage("Loupedeck.TwitchPlugin.metadata.Icon16x16.png");
            this.Info.Icon32x32 = EmbeddedResources.ReadImage("Loupedeck.TwitchPlugin.metadata.Icon32x32.png");
            this.Info.Icon48x48 = EmbeddedResources.ReadImage("Loupedeck.TwitchPlugin.metadata.Icon48x48.png");
            this.Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.TwitchPlugin.metadata.Icon256x256.png");
        }

        private void ConnectionCheckTimerOnElapsed(Object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this._connectionCheckTimer.Stop();
                if (TwitchPlugin.Proxy.IsConnected || String.IsNullOrEmpty(this._twitchAccount.AccessToken))
                {
                    this._connectionCheckTimer.Start();
                    return;
                }

                if (this._authenticationServer.IsTokenValid(this._twitchAccount.AccessToken))
                {
                    TwitchPlugin.PluginLog.Info(
                        "TwitchPlugin ConnectionCheckTimerOnElapsed: token is valid, connecting TwitchWrapper");

                    _ = TwitchPlugin.Proxy.Connect(this._twitchAccount.AccessToken);
                    return;
                }

                TwitchPlugin.PluginLog.Info("TwitchPlugin ConnectionCheckTimerOnElapsed: token invalid");
                this.RequestRefreshAccessToken(this._twitchAccount.RefreshToken);
            }
            catch (Exception ex)
            {
                TwitchPlugin.PluginLog.Error(ex,$"TwitchPlugin ConnectionCheckTimerOnElapsed error: {ex.Message}");
            }
        }

        private static TimeSpan ParseTimeSpan(String str)
        {
            switch (str)
            {
                case "10m":
                    return TimeSpan.FromMinutes(10);
                case "30m":
                    return TimeSpan.FromMinutes(30);
                case "1h":
                    return TimeSpan.FromHours(1);
                case "1d":
                    return TimeSpan.FromDays(1);
                case "1w":
                    return TimeSpan.FromDays(7);
                case "1mo":
                    return TimeSpan.FromDays(30);
                case "3mo":
                    return TimeSpan.FromDays(90);
                default:
                    return TimeSpan.Zero;
            }
        }
        internal const String ImageResPrefix = "Loupedeck.TwitchPlugin.Icons._80x80.";
        public static void Trace(String line) => TwitchPlugin.PluginLog.Info("TW:" + line); 

    }


}