namespace Loupedeck.TwitchPlugin
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using TwitchLib.Api.Auth;

    public class AuthenticationServer : IDisposable
    {
        private const String ResponseString = "<html><head><meta http-equiv=\"refresh\" content=\"0;URL='https://login.loupedeck.com/static-pages/signin-success.html?service=twitch'\" /></head><body>Authenticated</body></html>";
        // https://dev.twitch.tv/docs/api/reference/#update-chat-settings

        private static readonly List<String> Scopes = new List<String>
        {
            "clips:edit",
            "channel:edit:commercial",
            "user:edit",
            "chat:edit",
            "chat:read",
            "channel:moderate",
            "user:edit:broadcast",
            "channel_commercial", // this is scope from twitch API v5. Rfc does not currently allow to use /commercial command without it. This will probably brake when twitch drop support for API v5.
            "channel_editor", // This will probably brake when twitch drop support for API v5.
            "user:read:email"
        };

        private static Boolean IsSameScope(List<String> anotherScopeList) => AuthenticationServer.Scopes.Intersect(anotherScopeList).Count() == Scopes.Count;

        private readonly HttpListener _listener;
        private String _redirectUrl;
        private String _clientId;
        private String _clientSecret;
        private List<Int32> _authPorts;

        public EventHandler<AuthCodeResponse> TokenReceived { get; set; }

        public void ConfigureListener(TwitchPluginConfig config)
        {
            TwitchPlugin.PluginLog.Info("Twitch AuthenticationServer Start");
            this._authPorts = config.Ports;
            this._clientId = config.ClientId;
            this._clientSecret = config.ClientSecret;
        }

        // Actually starts
        private Boolean StartListener()
        {
            if (this._listener.IsListening)
            {

                TwitchPlugin.PluginLog.Error("Twitch AuthenticationServer: Already listening!");
                return false;
            }

            try
            {
                if (!NetworkHelpers.TryGetFreeTcpPort(this._authPorts, out var port))
                {
                    TwitchPlugin.PluginLog.Error("Twitch AuthenticationServer: No available ports for Twitch!");
                    return false;
                }

                this._redirectUrl = $"http://localhost:{port}/";
                this._listener.Prefixes.Clear();
                this._listener.Prefixes.Add(this._redirectUrl);
                this._listener.Start();
                this._listener.BeginGetContext(this.ListenerCallback, this._listener);
                return true;
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e, $"Twitch AuthenticationServer Start error: {e.Message}");
                return false;
            }
        }

        public AuthenticationServer()
        {
            this._listener = new HttpListener();
        }

        public void StartAuthtentication()
        {
            TwitchPlugin.PluginLog.Info("Twitch AuthenticationServer Authenticate");

            if (!this.StartListener())
            {
                TwitchPlugin.PluginLog.Error("Twitch AuthenticationServer: Listener has not been started yet.");
                return;
            }
            /* we are using OAuth Authorization code flow: https://dev.twitch.tv/docs/authentication/getting-tokens-oidc/#oidc-authorization-code-grant-flow 
             * 
             * 2-step authorization: 
             * 1. We authorize the app (and listen on redirect URL for authorization dode (see ListenerCallback)
             * 2. Once we receive the code, we form a HTTP Post Request and, if successful, receive all tokens (access,refresh and ID)
             */

            //Note: Using TwitchApi method to create oauth url did not work out because of scopes ... seems that no all the scopes needed are available there.
             
            var oauthUrl = "https://id.twitch.tv/oauth2/authorize?response_type=code&client_id=" + this._clientId + $"&redirect_uri={this._redirectUrl}" + "&scope=" + String.Join("+", Scopes);

            try
            {
                if (Helpers.IsWindows())
                {
                    System.Diagnostics.Process.Start(oauthUrl);
                }
                else
                {
                    System.Diagnostics.Process.Start("open", oauthUrl);
                }
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e, $"Twitch AuthenticationServer Authenticate error opening browser: {e.Message}");
            }
        }

        public void RefreshAccessToken(String refreshToken)
        {

            TwitchPlugin.PluginLog.Info("Twitch AuthenticationServer RefreshAccessToken");

            try
            {
                //Note we calling .Result of async method -- we don't need asynchronicity here
                var refresh = TwitchPlugin.Proxy.twitchApi.Auth.RefreshAuthTokenAsync(refreshToken, this._clientSecret).Result;
                TwitchPlugin.Proxy.twitchApi.Settings.AccessToken = refresh.AccessToken;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("400"))
                {
                    throw new InvalidAccessTokenException();
                }

                TwitchPlugin.PluginLog.Error(e, e.Message);
                throw;
            }
        }
        
        public enum TokenStatus
        {
            TOKEN_OK,
            TOKEN_INVALID,
            TOKEN_INVALID_SCOPE, /*This one should not happen in normal life.*/
        }

        public TokenStatus IsTokenValid(String accessToken)
        {
            TokenStatus status = TokenStatus.TOKEN_OK;
            try
            {
                var tokenInfo = TwitchPlugin.Proxy.twitchApi.Auth.ValidateAccessTokenAsync(accessToken).Result;
                if(tokenInfo == null)
                {
                    status = TokenStatus.TOKEN_INVALID;
                    TwitchPlugin.PluginLog.Warning("Server replied, invalid token");
                }
                else
                {
                    if (!AuthenticationServer.IsSameScope(tokenInfo.Scopes))
                    {
                        status = TokenStatus.TOKEN_INVALID_SCOPE;
                        TwitchPlugin.PluginLog.Warning("Invalid token: Scope differs");
                    }

                    //FIXME: Make use of the the EXPIRES_IN clause
                    TwitchPlugin.PluginLog.Info($"Validated token: Expired in {tokenInfo.ExpiresIn}");
                }

            }
            catch (Exception ex)
            {
                status = TokenStatus.TOKEN_INVALID;
                TwitchPlugin.PluginLog.Error(ex,"Twitch AuthenticationServer: Invalid access token.");
            }

            return status;
        }

        public void Dispose() => ((IDisposable)this._listener)?.Dispose();

        private async void ListenerCallback(IAsyncResult result)
        {
            TwitchPlugin.PluginLog.Info("Twitch AuthenticationServer ListenerCallback");

            try
            {
                var listener = (HttpListener)result.AsyncState;
                if (!listener.IsListening)
                {
                    TwitchPlugin.PluginLog.Warning("Go to ListenerCallback but Listener is not listening ?");
                    return;
                }

                var context = listener.EndGetContext(result);
                var code = context.Request.QueryString["code"];
                using (var client = new HttpClient())
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    //FIXME !! For whatever reason, whenever GetAccessTokenFromCodeAsync is used, the response ('success') can no longer be written!
                    //var response = await TwitchPlugin.Proxy.twitchApi.Auth.GetAccessTokenFromCodeAsync(code, this._clientSecret, this._redirectUrl);

                    var responseFromServer = await client.PostAsync(new Uri("https://id.twitch.tv/oauth2/token?"),
                        new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<String, String>("client_id", this._clientId),
                            new KeyValuePair<String, String>("client_secret", this._clientSecret),
                            new KeyValuePair<String, String>("grant_type", "authorization_code"),
                            new KeyValuePair<String, String>("redirect_uri", this._redirectUrl),
                            new KeyValuePair<String, String>("code", code)
                        }));
  
                    var response = await responseFromServer.Content.ReadAsStringAsync();
  
                    this.TokenReceived?.Invoke(this, JsonConvert.DeserializeObject<AuthCodeResponse>(response));

                    //this.TokenReceived?.Invoke(this, response /*JsonConvert.DeserializeObject<AuthCodeResponse>(response)*/);

                    await writer.WriteAsync(ResponseString);
                }
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e, $"Twitch AuthenticationServer callback error: {e.Message}");
            }
            finally
            {
                try
                {
                    if (this._listener.IsListening)
                    {
                        this._listener.Stop();
                    }
                }
                catch (Exception ex)
                {
                    TwitchPlugin.PluginLog.Error(ex, "Twitch AuthenticationServer callback error in 'finally'");
                }
            }
        }

        public static ValidateAccessTokenResponse GetTokenInfo(String token)
        {
            return TwitchPlugin.Proxy.twitchApi.Auth.ValidateAccessTokenAsync(token).Result;
        }
    }
}
/**     Authentication scopes
 *        See https://dev.twitch.tv/docs/api/migration/
            and https://dev.twitch.tv/docs/authentication/scopes/

          Elgato, for example, requests the following: 
                Log into chat and send messages
                Update your channel's title, game, status, and other metadata
                Cut VODs
                Edit your channel's broadcast configuration including extension activations
                Run commercials on a channel
                Create clips from a broadcast or video
                View your email address
        };
         */
