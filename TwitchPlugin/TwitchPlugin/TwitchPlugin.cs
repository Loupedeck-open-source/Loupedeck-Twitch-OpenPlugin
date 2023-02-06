namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using TwitchLib.Api.Auth;
    using static Loupedeck.TwitchPlugin.AuthenticationServer;
    using TwitchLib.Api.Helix.Models.Entitlements;
    using TwitchLib.Client.Events;

    public class TwitchPlugin : Plugin
    {
        private const Int32 DefaultConnectionTimerInterval = 5000;
        public static PluginLogFile PluginLog { get; private set; } = null;

        private const String ConfigFileName = "twitch-v2.json";

        private readonly PluginPreferenceAccount _twitchAccount;

        private TwitchPluginConfig _pluginConfig = null;

        public override Boolean UsesApplicationApiOnly => true;
        public override Boolean HasNoApplication => true;

        internal static TwitchProxy Proxy { get; private set; }

        public TwitchPlugin()
        {
            if (TwitchPlugin.PluginLog == null)
            {
                TwitchPlugin.PluginLog = this.Log;
            }

            this._twitchAccount = new PluginPreferenceAccount("twitch-account")
            {
                DisplayName = "Twitch Account",
                IsRequired = true,
                LoginUrlTitle = "Sign in to Twitch",
                LogoutUrlTitle = "Sign out from Twitch"
            };

            this.PluginPreferences.Add(this._twitchAccount);

            TwitchPlugin.Proxy = new TwitchProxy();
        }


        public override void Load()
        {
            base.Load();
            this.LoadPluginIcons();

            TwitchPlugin.Proxy.AppConnected += this.OnConnected;
            TwitchPlugin.Proxy.AppDisconnected += this.OnDisconnected;
            TwitchPlugin.Proxy.TokensUpdated += this.OnTokensUpdated;

            TwitchPlugin.Proxy.ConnectionError += this.OnConnectionError;
            TwitchPlugin.Proxy.Error += this.OnError;
            TwitchPlugin.Proxy.IncorrectLogin += this.OnIncorrectLogin;

            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOn += this.UpdateFollowersBitmap;
            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOff += this.UpdateFollowersBitmap;
            TwitchPlugin.Proxy.AppEvtChatSlowModeOn+= this.UpdateSlowModeBitmap;
            TwitchPlugin.Proxy.AppEvtChatSlowModeOff += this.UpdateSlowModeBitmap;

            this._twitchAccount.LoginRequested += this.OnTwitchAccountOnLoginRequested;
            this._twitchAccount.LogoutRequested += this.OnTwitchAccountOnLogoutRequested;
            
            this.ServiceEvents.OnlineFileContentReceived += this.OnOnlineFileContentReceived;
            this.ServiceEvents.GetOnlineFileContent(ConfigFileName);
        }

        public override void Unload()
        {
            TwitchPlugin.Proxy.AppConnected -= this.OnConnected;
            TwitchPlugin.Proxy.AppDisconnected -= this.OnDisconnected;
            TwitchPlugin.Proxy.TokensUpdated -= this.OnTokensUpdated;
            TwitchPlugin.Proxy.ConnectionError -= this.OnConnectionError;
            TwitchPlugin.Proxy.Error -= this.OnError;
            TwitchPlugin.Proxy.IncorrectLogin -= this.OnIncorrectLogin;

            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOn -= this.UpdateFollowersBitmap;
            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOff -= this.UpdateFollowersBitmap;
            TwitchPlugin.Proxy.AppEvtChatSlowModeOn -= this.UpdateSlowModeBitmap;
            TwitchPlugin.Proxy.AppEvtChatSlowModeOff -= this.UpdateSlowModeBitmap;

            this._twitchAccount.LoginRequested -= this.OnTwitchAccountOnLoginRequested;
            this._twitchAccount.LogoutRequested -= this.OnTwitchAccountOnLogoutRequested;
            this.ServiceEvents.OnlineFileContentReceived -= this.OnOnlineFileContentReceived;
            TwitchPlugin.Proxy.Dispose();
        }
        //Temporary stubs before followers and subscribers action are ready
        private void UpdateFollowersBitmap(Object sender, EventArgs e)
        {
            this.OnActionImageChanged("ToggleFollowersOnlyList", String.Empty);
            this.OnActionImageChanged("ToggleFollowersOnly", String.Empty);
        }

        //Temporary stubs before followers and subscribers action are ready        
        private void UpdateSlowModeBitmap(Object sender, EventArgs e)
        {
            this.OnActionImageChanged("ToggleSlowChat", String.Empty);
            this.OnActionImageChanged("ToggleSlowChatList", String.Empty);
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

        protected override Boolean TryGetActionImage(String actionName, String actionParameter, PluginImageSize imageSize,
            out BitmapImage bitmap)
        {
            BitmapImage GetViewersBitmapWithText(String imageName, String buttonText, Int32 fontSize = 15, BitmapColor? color = null)
            {
                using (var bitmapBuilder = new BitmapBuilder(imageSize))
                {
                    bitmapBuilder.DrawLibraryImage(imageName, imageSize);

                    if (!String.IsNullOrEmpty(buttonText))
                    {
                        bitmapBuilder.DrawText(buttonText, 10, 30, 60, 60, color: color, fontSize: fontSize); //, fontSize: 9);
                    }

                    return bitmapBuilder.ToImage();
                }
            }

            switch (actionName)
            {
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
                case "ToggleFollowersOnly":
                    imageFileName = TwitchPlugin.Proxy.FollowersOnly == TimeSpan.Zero
                        ? "Twitch/TwitchFollowerChatToggle.png"
                        : "Twitch/TwitchFollowerChat.png";
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
                case "ToggleFollowersOnly":
                    TwitchPlugin.Proxy.SendMessage(TwitchPlugin.Proxy.IsFollowersOnly
                        ? ".followersoff"
                        : ".followers 10m");
                    break;
                case "RunCommercial":
                    TwitchPlugin.Proxy.SendMessage(".commercial");
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
                    TwitchPlugin.Proxy.ResetAllSettings();
                    break;
                default:
                    TwitchPlugin.PluginLog.Info($"ProcessCommand - Default {commandName}");
                    break;
            }
        }

        private void OnTwitchAccountOnLoginRequested(Object sender, EventArgs e)
        {
            //User pressed 'Login' 
            TwitchPlugin.PluginLog.Info("TwitchPlugin OnTwitchAccountOnLoginRequested");
            
            TwitchPlugin.Proxy.StartAuthentication();
        }

        private void OnTwitchAccountOnLogoutRequested(Object sender, EventArgs e)
        {
            //User pressed 'Logout' 
            TwitchPlugin.PluginLog.Info("TwitchPlugin OnTwitchAccountOnLogoutRequested");

            // Asking Proxy here.,
            this._twitchAccount.AccessToken = null;
            this._twitchAccount.RefreshToken = null;
            TwitchPlugin.Proxy.Disconnect();

            this._twitchAccount.ReportLogout(); //So that we force login for the next time
        }

        private void OnError(Object sender, Exception e)
        {
            TwitchPlugin.PluginLog.Warning(e,$"TwitchAPI OnError received: {e.Message}");


            /*
             *          FIXME: Should we set a Plugin state here instead? 
             *          
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
            */
        }

        private void OnConnectionError(Object sender, String message)
        {
            TwitchPlugin.PluginLog.Info($"Connection Error: {message}");
        }

        private void OnDisconnected(Object sender, EventArgs e)
        {
            TwitchPlugin.PluginLog.Info("OnDisconnected");
            //Note that, except for the Forced logout and closing te application 
            //User never disconnects from Twitch. 
        }

        private void OnIncorrectLogin(Object sender, (String, Exception) e)
        {
            var (_, ex) = e;

            TwitchPlugin.PluginLog.Warning(ex,$"Incorrect Login: {ex.Message}");
            
            // Incorrect login happens when we are re-authenticating. Let's ignore it for now
            this._twitchAccount.ReportLogout();
        }

        private void OnTokensUpdated(Object sender, TokensUpdatedEventArg args)
        {
            //Note we are coming here also after successful token refresh. 
            TwitchPlugin.PluginLog.Info($"Twitch tokens updated for : {args.UserName}");

            this._twitchAccount.AccessToken = args.AccessToken;
            this._twitchAccount.RefreshToken = args.RefreshToken;

            //Setting status and storing the tokens
            this._twitchAccount.ReportLogin(args.UserName, this._twitchAccount.AccessToken, this._twitchAccount.RefreshToken);
        }

        private void OnConnected(Object sender, EventArgs e)
        {
            TwitchPlugin.PluginLog.Info($"Connected to twitch client");

            this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, "Connected!");
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

                //TODO: Document that for 3rd party
                if (!this.TryGetPluginSetting(ConfigFileName, out json))
                {
                    TwitchPlugin.PluginLog.Error("TwitchPlugin OnOnlineFileContentReceived: couldn't read config file from cache");
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "No connection to Loupedeck Server.", null,
                        null);

                    return;
                }

                pluginConfig = JsonHelpers.DeserializeObject<TwitchPluginConfig>(json);

                this._pluginConfig = pluginConfig;
            }
            else
            {
                this.SetPluginSetting(ConfigFileName, json);
            }

            TwitchPlugin.Proxy.SetClientCredentials(pluginConfig.ClientId, pluginConfig.ClientSecret);
            TwitchPlugin.Proxy.SetPorts(pluginConfig.Ports);


            if (!String.IsNullOrEmpty(this._twitchAccount.AccessToken) &&
                !String.IsNullOrEmpty(this._twitchAccount.RefreshToken)
                && TwitchProxy.ValidateAccessToken(this._twitchAccount.AccessToken, out var validationResp) )
            {
                TwitchPlugin.PluginLog.Info("Attempting to connect with cached tokens");
                Proxy.PreconfiguredConnect(this._twitchAccount, validationResp);
            }
            else
            {
                this._twitchAccount.AccessToken = null;
                this._twitchAccount.RefreshToken = null;
                
                TwitchPlugin.PluginLog.Info("TwitchPlugin OnOnlineFileContentReceived: token not cached or invalid. Need manual relogin");
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

        private static readonly BitmapColor BitmapColorPink = new BitmapColor(255, 192, 203);

        internal BitmapBuilder BuildImage(PluginImageSize imageSize, String imageName, String text, Boolean selected)
        {
            var bitmapBuilder = new BitmapBuilder(imageSize);
            try
            {

                var image = EmbeddedResources.ReadImage(imageName);
                bitmapBuilder.DrawImage(image);
            }
            catch (Exception ex)
            {
                this.Log.Error($"Cannot load image {imageName}, exception {ex}");
            }

            if (!String.IsNullOrEmpty(text))
            {
                var x1 = bitmapBuilder.Width * 0.1;
                var w = bitmapBuilder.Width * 0.8;
                var y1 = bitmapBuilder.Height * 0.60;
                var h = bitmapBuilder.Height * 0.3;

                bitmapBuilder.DrawText(text, (Int32)x1, (Int32)y1, (Int32)w, (Int32)h,
                                            selected ? BitmapColor.Black : BitmapColorPink,
                                            imageSize == PluginImageSize.Width90 ? 13 : 9,
                                            imageSize == PluginImageSize.Width90 ? 12 : 8, 1);
            }

            return bitmapBuilder;
        }
        
        /// <summary>
        ///  Draws text over the bitmap. Bad location but in absence of the better components, put it here.
        /// </summary>
        /// <param name="imageSize">size of the image</param>
        /// <param name="imagePath">Image file name</param>
        /// <param name="text">text to render</param>
        /// <param name="textSelected">If true, darker color of text is chosen</param>
        /// <returns>bitmap with text rendered</returns>
        internal BitmapImage GetPluginCommandImage(PluginImageSize imageSize, String imagePath, String text = null, Boolean textSelected = false) =>
            this.BuildImage(imageSize, ImageResPrefix + imagePath, text, textSelected).ToImage();


    }
}