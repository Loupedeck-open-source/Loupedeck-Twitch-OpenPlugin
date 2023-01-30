
namespace Loupedeck.TwitchPlugin
{ 

    using System;
    using System.Collections.Generic;

    using TwitchLib.Api.Auth;
    using TwitchLib.Client.Events;
    using TwitchLib.Client.Models;
    using TwitchLib.Communication.Events;

    // Plugin class will only take care of passing 'Login requested' and 'Logout requested' 

    // We will communicate statuses to the plugin via events 

    // Connection monitor and authentication server will be parts of the Proxy
    //  -- Check if connection monitor is actually needed! 
    // 

    // Before connecting, token is validated and validation result is saved. We can optionally arm the timer to refresh the token before it expires
    //  -> UserId is taken from the token validation result

    //  ClientID and ClientSecret, Access Token and Refresh Tokens are stored in twitchApi.Settings
    //      -> Other means are just providing values for it.
    //  
    // 
    //  
    public partial class TwitchProxy : IDisposable
    {
        public event EventHandler<TokensUpdatedEventArg> TokensUpdated;
        public event EventHandler<EventArgs> AppConnected;
        public event EventHandler<String> ConnectionError;
        public event EventHandler<EventArgs> OnTwitchAccessTokenExpired;
        public event EventHandler<EventArgs> AppDisconnected;
        public void SetPorts(List<Int32> ports) => this._authServer.SetPorts(ports);

        private readonly AuthenticationServer _authServer;

        public Boolean IsConnected => (this._twitchClient?.IsConnected ?? false) == true;
        
        public EventHandler<(String, Exception)> IncorrectLogin { get; set; }
        
        private String RefreshToken { get; set; } = null;

        private readonly System.Timers.Timer _refreshTokenTimer = null; 

        private void SetRefreshToken(String refreshToken,Int32 expiresIn)
        {
            this._refreshTokenTimer.Enabled = false;

            var refreshIntervalMs = 1000 * (expiresIn > 30 ? (expiresIn - 20) : 10) * 0.6;

            //Say, 20 seconds before an actual expiration. 
            this._refreshTokenTimer.Interval = refreshIntervalMs;
            this.RefreshToken = refreshToken;
            this._refreshTokenTimer.Enabled = true;

            TwitchPlugin.PluginLog.Info($"Refresh timer interval {refreshIntervalMs/1000}s, expected to fire on {DateTime.Now.ToLocalTime().AddSeconds(refreshIntervalMs/1000)})");
        }

        private void OnRefreshTokenTimerTick(Object _, Object _1)
        {
            TwitchPlugin.PluginLog.Info("It's time to refresh the token!");
            this.RequestRefreshAccessToken(this.RefreshToken);
        }

        /* Starts an authentication process, upon successful completion of which the connection should start */
        public void StartAuthentication()
        {
            TwitchPlugin.PluginLog.Info("Starting authentication...");
            //Starting authentication process.  Once completed, OnAccessTokenReceived event is fired, and we can continue. On error, we will get OnAccessTokenError
            this._authServer.StartAuthtentication();
            //This one should set plugin status to error
            //                this.ConnectionError?.Invoke(this, "Cannot Authenticate with Twitch service. Please restart Loupedeck and try again.");
        }

        public static Boolean ValidateAccessToken(String token, out ValidateAccessTokenResponse response)
        {
            var success = Helpers.TryExecuteFunc(() => { return TwitchPlugin.Proxy.twitchApi.Auth.ValidateAccessTokenAsync(token).Result; }, out var resp);
            response = resp;
            if (success && response != null)
            {
                TwitchPlugin.PluginLog.Info($"Token validated: User {response.UserId}, Expires in {response.ExpiresIn} seconds or { response.ExpiresIn/60} minutes");
            }

            return success && response!=null;
        }

        public void PreconfiguredConnect(PluginPreferenceAccount account,ValidateAccessTokenResponse validate) => 
            this.OnAccessTokenReceived(this, new AccessTokenReceivedEventArgs(account.AccessToken, account.RefreshToken, validate));
        

        private void OnAccessTokenReceived(Object sender, AccessTokenReceivedEventArgs arg )
        {
            TwitchPlugin.PluginLog.Info("Access token received");
            if (!String.IsNullOrEmpty(this.twitchApi.Settings.AccessToken) && this.IsConnected)
            {
                TwitchPlugin.PluginLog.Info("Already connected, and access token non-empty.");
                return;
            } 
            else if( this.IsConnected )
            {
                TwitchPlugin.PluginLog.Info("Already connected, access token empty.");
                // WE land here when we are reconnecting with new asccess tokens!
                this.Disconnect(); 
            }

            if(this._twitchClient == null)
            {
                this.InitializeTwitchClient();
            }

            this._userInfo = new UserInfo(arg.UserId, arg.Login);

            //Received access token! Can proceed with connecting
            this.twitchApi.Settings.AccessToken = arg.AccessToken;

            //Storing refresh token. We will store that and this.twitchApi.Settings.AccessToken to account upon succesful connection 
            this.SetRefreshToken(arg.RefreshToken, arg.ExpiresIn);

            //Updating plugin so that tokens will be stored
            this.TokensUpdated?.Invoke(sender, new TokensUpdatedEventArg(this._userInfo.Login, this.twitchApi.Settings.AccessToken, this.RefreshToken));

            //Note: We need to verify situation when DoConnect fails but authentication succeeds \
            if (Helpers.TryExecuteFunc(() => { return this.DoConnect(); }, out var connResult) && connResult)
            {
                //Connected successfully, we should expect 'Connected' callback now!

                /* One of the following will fire
                    this.OnTwitchClientConnected
                    this.OnTwitchIncorrectLogin
                    this.OnTwitchConnectionError
                */

                TwitchPlugin.PluginLog.Info("Connected successfully");
            }
            else
            {
                TwitchPlugin.PluginLog.Info("Error connecting");
            }
        }

        private void OnTwitchClientConnected(Object sender, OnConnectedArgs e)
        {
            TwitchPlugin.PluginLog.Info($"Twitch client connected");
            this.AppConnected?.Invoke(sender, null);
        }

        private void OnTwtchClientReconnected(Object sender, OnReconnectedEventArgs e)
        {
            TwitchPlugin.PluginLog.Info($"Twitch client reconnected");
            this.OnTwitchClientConnected(sender, null);
        }
        

        private void OnAccessTokenError(Object sender, AccessTokenErrorEventArgs arg)
        {
            //ERROR WITH ACCESS TOKEN.  We should logout and tell user
            TwitchPlugin.PluginLog.Warning($"Access token error {arg.errorCode}");

            this.IncorrectLogin?.Invoke(this, (this._userInfo.Login, new Exception($"Access token error {arg.errorCode}")));
        }

        private Boolean DoConnect()
        {
            TwitchPlugin.PluginLog.Info("TwitchPlugin Connect");

            this._twitchClient.Initialize(
                    credentials: new ConnectionCredentials(this._userInfo.Login, this.twitchApi.Settings.AccessToken),
                    channel:this._userInfo.Login/*,
                    autoReListenOnExceptions:false */);

            if (!this._twitchClient.Connect())
            {
                TwitchPlugin.PluginLog.Error("Error executing twitchClient.Connect");
                return false;
            }

            return true;
        }

        public void SetClientCredentials(String clientID, String clientSecret)
        {
            this.twitchApi.Settings.ClientId = clientID;
            this.twitchApi.Settings.Secret = clientSecret;
        }
        
        public void Disconnect()
        {
            TwitchPlugin.PluginLog.Info("TwitchPlugin Disconnect");

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

                this.DisposeTwitchClient();
                
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e, $"TwitchPlugin Disconnect error: {e.Message}");
                throw;
            }
        }

        private void OnTwitchConnectionError(Object sender, OnConnectionErrorArgs e)
        {
            TwitchPlugin.PluginLog.Warning($"OnTwitchConnectionError: {e.Error.Message}");

            this.ConnectionError?.Invoke(sender, e.Error.Message);
        }


        private void OnTwitchClientDisconnected(Object sender, OnDisconnectedEventArgs e)
        {
            //We receive that, in particular when access token expires. 
            //We also receive that if internet disappears

            TwitchPlugin.PluginLog.Warning($"OnTwitchClientDisconnected");

            this.AppDisconnected?.Invoke(sender, EventArgs.Empty);
        }

        private void OnTwitchIncorrectLogin(Object sender, OnIncorrectLoginArgs e)
        {
            //ASSUMING we are now handling token expiration correctly, IncorrectLogin
            //means we must re-login manually  (whether oauth was revoked or password changed)

            // NOTE: DISCONNECT IS NOT WORKING, See reference to TwitchLib bug this.Disconnect();
            //Hence we will need to KILL twitch clientt
            this.DisconnectAndKillTwitchClient();

            this.IncorrectLogin?.Invoke(sender, (e.Exception.Username, e.Exception));
        }


        private Boolean _refreshAccessTokenRequestBlocked = false;

        private void OnAccessTokenExpired(Object sender, EventArgs e)
        {
            TwitchPlugin.PluginLog.Info("TwitchPlugin OnAccessTokenExpired");
            this.RequestRefreshAccessToken(this.RefreshToken);
        }

        private void DisconnectAndKillTwitchClient()
        {
            // As a (hopefully) temporary solution to an issue in TwitchLib  https://github.com/TwitchLib/TwitchLib/issues/1104
            // we need to kill client instead of disconnecting  (because reconnection kicks in)
            // There is some risk for that operation to run sync 
            TwitchPlugin.PluginLog.Info("Disconnecting and killing twitch client!");
            //Informing others we're going down
            
            this.AppDisconnected?.BeginInvoke(this, EventArgs.Empty);
            this.DisposeTwitchClient();
            this._twitchClient = null;
        }

        private void RequestRefreshAccessToken(String inRefreshToken=null)
        {
            var refreshToken = String.IsNullOrEmpty(inRefreshToken) ? this.RefreshToken : inRefreshToken;

            TwitchPlugin.PluginLog.Info("TwitchPlugin RequestRefreshAccessToken");

            if (this._refreshAccessTokenRequestBlocked)
            {
                TwitchPlugin.PluginLog.Warning("TwitchPlugin Already requesting");
                return;
            }

            this._refreshAccessTokenRequestBlocked = true;

            try
            {
                var result = TwitchPlugin.Proxy.twitchApi.Auth.RefreshAuthTokenAsync(refreshToken, this.twitchApi.Settings.Secret).Result;

                if( result != null)
                {
                    TwitchPlugin.PluginLog.Info($"Tokens refreshed successfully, new token expires in {result.ExpiresIn}s or {result.ExpiresIn/60}min ");

                    this.twitchApi.Settings.AccessToken = result.AccessToken;


                    //See above the note about reconnection bug
                    /*this.DisconnectAndKillTwitchClient();
                        TwitchPlugin.PluginLog.Info("Phoneix rises from the ashes!");
                        this.InitializeTwitchClient();
                        this.DoConnect();
                    */

                    this._twitchClient.Initialize(
                        credentials: new ConnectionCredentials(this._userInfo.Login, this.twitchApi.Settings.AccessToken),
                        channel: this._userInfo.Login/*,
                        autoReListenOnExceptions:false */);

                    this._twitchClient.Reconnect();


                    this.SetRefreshToken(result.RefreshToken, result.ExpiresIn);
                    //Note that in Refresh response we naturally don't receive userid/login - we use the same as before. 
                    //this.OnAccessTokenReceived(this, new AccessTokenReceivedEventArgs(result.AccessToken, result.RefreshToken, this._userInfo.Id, this._userInfo.Login, result.ExpiresIn));
                    
                    this.TokensUpdated?.Invoke(this, new TokensUpdatedEventArg(this._userInfo.Login, this.twitchApi.Settings.AccessToken, this.RefreshToken));
                }
                else
                {
                    throw new InvalidAccessTokenException();
                }
                
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Warning(e, "Twitch: Failed to refresh token. Manual login is needed");
                //To reset the access tokens
                this.IncorrectLogin.Invoke(this, (this._userInfo.Login, e));
                
            }
            finally
            {
                this._refreshAccessTokenRequestBlocked = false;
            }
            
        }
    }

}
