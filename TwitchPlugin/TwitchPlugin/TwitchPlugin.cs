﻿namespace Loupedeck.TwitchPlugin
{
    using System;

    public class TwitchPlugin : Plugin
    {
        public static PluginLogFile PluginLog { get; private set; } = null;

        private readonly String SupportPageUrl = "https://support.logi.com/hc/articles/25522063648407-Other-Plugins-MX-Creative-Console#h_01J4V10DCGH4V2HFVRMXVMWFF2";
        private const String ConfigFileName = "twitch-v2.json";

        private readonly PluginPreferenceAccount _twitchAccount;
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

            this._twitchAccount.LoginRequested += this.OnTwitchAccountOnLoginRequested;
            this._twitchAccount.LogoutRequested += this.OnTwitchAccountOnLogoutRequested;
            
            this.ServiceEvents.OnlineFileContentReceived += this.OnOnlineFileContentReceived;
            this.ServiceEvents.GetOnlineFileContent(ConfigFileName);

            //Intialization will contunue in this.OnOnlineFileContentReceived
        }

        public override void Unload()
        {
            TwitchPlugin.Proxy.AppConnected -= this.OnConnected;
            TwitchPlugin.Proxy.AppDisconnected -= this.OnDisconnected;
            TwitchPlugin.Proxy.TokensUpdated -= this.OnTokensUpdated;
            TwitchPlugin.Proxy.ConnectionError -= this.OnConnectionError;
            TwitchPlugin.Proxy.Error -= this.OnError;
            TwitchPlugin.Proxy.IncorrectLogin -= this.OnIncorrectLogin;

            this._twitchAccount.LoginRequested -= this.OnTwitchAccountOnLoginRequested;
            this._twitchAccount.LogoutRequested -= this.OnTwitchAccountOnLogoutRequested;
            this.ServiceEvents.OnlineFileContentReceived -= this.OnOnlineFileContentReceived;
            TwitchPlugin.Proxy.Dispose();
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

        private void ProcessCommand(String commandName, String parameter)
        {
            switch (commandName)
            {
                case "SendChatMessage":
                    TwitchPlugin.Proxy.SendMessage(parameter);
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
            TwitchPlugin.PluginLog.Info("TwitchPlugin OnTwitchAccountOnLogoutRequested: Reporting Logout");

            // Asking Proxy here.,

            TwitchPlugin.Proxy.Disconnect();
            TwitchPlugin.PluginLog.Info("Reporting Logout");
            this.CleanTokensAndLogout();
        }

        private void OnError(Object sender, Exception e) => TwitchPlugin.PluginLog.Warning(e, $"TwitchAPI OnError received: {e.Message}");

        private void OnConnectionError(Object sender, String message) => TwitchPlugin.PluginLog.Info($"Connection Error: {message}");
        
        //Note that, except for the Forced logout and closing te application 
        //User never disconnects from Twitch. 
        private void OnDisconnected(Object sender, EventArgs e) => TwitchPlugin.PluginLog.Info("OnDisconnected");

        // How many times we got 'incorrect login' error - this might be an error related to re-authentication
        // Reset on successful connect
        private UInt32 _incorrectLoginErrors = 0;
        private const UInt32 MaxIncorrectLoginErrors = 10; 

        private void OnIncorrectLogin(Object sender, (String, Exception) e)
        {
            var (_, ex) = e;

            TwitchPlugin.PluginLog.Warning(ex,$"Incorrect Login: {ex.Message}");
           
            if( this._incorrectLoginErrors++ > MaxIncorrectLoginErrors )
            {
                TwitchPlugin.PluginLog.Warning("Too many incorrect logins. Reporting logout");
                this.CleanTokensAndLogout();
            }
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
            TwitchPlugin.PluginLog.Info($"Connected to twitch client. Reporting Login.");
            this._incorrectLoginErrors = 0;
            this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, "Connected!");
        }

        private void CleanTokensAndLogout()
        {
            this._twitchAccount.AccessToken = null;
            this._twitchAccount.RefreshToken = null;
            TwitchPlugin.PluginLog.Info("Reporting Logout");
            this._twitchAccount.ReportLogout();
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
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "No connection to Logi Plugin Server.", this.SupportPageUrl,
                        null);

                    return;
                }

                pluginConfig = JsonHelpers.DeserializeObject<TwitchPluginConfig>(json);

            }
            else
            {
                this.SetPluginSetting(ConfigFileName, json);
            }

            TwitchPlugin.Proxy.SetClientCredentials(pluginConfig.ClientId, pluginConfig.ClientSecret);
            TwitchPlugin.Proxy.SetPorts(pluginConfig.Ports);

            if (!String.IsNullOrEmpty(this._twitchAccount.AccessToken)
                && !String.IsNullOrEmpty(this._twitchAccount.RefreshToken)
                && TwitchProxy.ValidateAccessToken(this._twitchAccount.AccessToken, out var validationResp) )
            {
                TwitchPlugin.PluginLog.Info("Attempting to connect with cached tokens");
                Proxy.PreconfiguredConnect(this._twitchAccount, validationResp);
            }
            else
            {
                if (!String.IsNullOrEmpty(this._twitchAccount.RefreshToken))
                {
                    try
                    {
                        // Validation of the access token has failed. Lets try to receive a new one.
                        TwitchPlugin.Proxy.RequestRefreshAccessToken(this._twitchAccount.RefreshToken);
                    }
                    catch (InvalidAccessTokenException ex)
                    {
                        TwitchPlugin.PluginLog.Info(ex, $"TwitchPlugin Twitch: Failed to refresh token. Manual login is needed.");
                        this.CleanTokensAndLogout();
                    }
                }
                else
                {
                    TwitchPlugin.PluginLog.Info($"TwitchPlugin OnOnlineFileContentReceived: token not cached or invalid. Need manual relogin. Access Token Empty={String.IsNullOrEmpty(this._twitchAccount.AccessToken)}. Refresh Token Empty={String.IsNullOrEmpty(this._twitchAccount.RefreshToken)}");
                    this.CleanTokensAndLogout();
                }
            }
        }

        private void LoadPluginIcons() => this.Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.TwitchPlugin.metadata.Icon256x256.png");

        internal const String ImageResPrefix = "Loupedeck.TwitchPlugin.Icons.";

        public static void Trace(String line) => TwitchPlugin.PluginLog.Info("TW:" + line);

        private static readonly BitmapColor BitmapColorPink = new BitmapColor(255, 192, 203);

        internal BitmapBuilder BuildImage(PluginImageSize imageSize, String imageName, String text, Boolean selected)
        {
            // Fix for bug when the value of imageSize does not belong to PluginImageSize enum
            switch ((Int32)imageSize)
            {
                case 60:
                    imageSize = PluginImageSize.Width60;
                    break;
                case 90:
                    imageSize = PluginImageSize.Width90;
                    break;
                case 116:
                    imageSize = PluginImageSize.Width116;
                    break;
                default:
                    break;
            }

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
                                            imageSize == PluginImageSize.Width90 ? 26 : 18,
                                            imageSize == PluginImageSize.Width90 ? 24 : 16, 1);
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