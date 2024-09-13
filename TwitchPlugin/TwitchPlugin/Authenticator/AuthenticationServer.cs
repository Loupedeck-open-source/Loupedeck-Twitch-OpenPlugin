namespace Loupedeck.TwitchPlugin
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web;
    using TwitchLib.Api.Auth;
   
    public class AuthenticationServer : IDisposable
    {
        private const String ResponseString = "<html><head><meta http-equiv=\"refresh\" content=\"0;URL='https://login.loupedeck.com/static-pages/signin-success.html?service=twitch'\" /></head><body>Authenticated</body></html>";
        // https://dev.twitch.tv/docs/api/reference/#update-chat-settings

        private String GetAuthErrorPage(String reason)=> $"<html><head><meta http-equiv=\"refresh\" content=\"10;URL='https://login.loupedeck.com/static-pages/signin-error.html?service=twitch&error={HttpUtility.UrlEncode(reason)}'\" /></head><body>Authentication Error: {reason}</body></html>";

        private static readonly List<String> Scopes = new List<String>
        {
            "clips:edit",
            "channel:edit:commercial",
            "user:edit",
            "chat:edit",
            "chat:read",
            "moderator:manage:chat_settings",
            "channel:moderate",
            "user:edit:broadcast",
            "moderator:manage:chat_messages", 
            "user:read:email",
            "moderator:manage:shield_mode"
        };

        public static Boolean IsSameScope(List<String> anotherScopeList) => AuthenticationServer.Scopes.Intersect(anotherScopeList).Count() == Scopes.Count;

        private readonly HttpListener _listener;
        public String RedirectUrl { get; private set; }

        private List<Int32> _authPorts;

        public event EventHandler<AccessTokenReceivedEventArgs> OnTokenReceived;
        public event EventHandler<AccessTokenErrorEventArgs>    OnTokenError;

        public void SetPorts(List<Int32> redirectPorts) => this._authPorts = redirectPorts;


        // Actually starts the HTTP listener 
        private Boolean StartListener()
        {
            if (this._listener.IsListening)
            {
                TwitchPlugin.PluginLog.Error("Twitch AuthenticationServer: Already listening!");
                return true;
            }

            try
            {
                if (!NetworkHelpers.TryGetFreeTcpPort(this._authPorts, out var port))
                {
                    TwitchPlugin.PluginLog.Error("Twitch AuthenticationServer: No available ports for Twitch!");
                    return false;
                }

                this.RedirectUrl = $"http://localhost:{port}/";
                this._listener.Prefixes.Clear();
                this._listener.Prefixes.Add(this.RedirectUrl);
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

        public AuthenticationServer() => this._listener = new HttpListener();

        public Boolean StartAuthtentication()
        {
            TwitchPlugin.PluginLog.Info("Twitch AuthenticationServer Authenticate");

            if (!this.StartListener())
            {
                //Probably need to fire Error event.
                TwitchPlugin.PluginLog.Error("Twitch AuthenticationServer: Error starting listener");
                return false;
            }
            /* we are using OAuth Authorization code flow: https://dev.twitch.tv/docs/authentication/getting-tokens-oidc/#oidc-authorization-code-grant-flow 
             * 
             * 2-step authorization: 
             * 1. We authorize the app (and listen on redirect URL for authorization dode (see ListenerCallback)
             * 2. Once we receive the code, we form a HTTP Post Request and, if successful, receive all tokens (access,refresh and ID)
             */

            //Note: Using TwitchApi method to create oauth url did not work out because of scopes ... seems that no all the scopes needed are available there.
                        
            var oauthUrl = "https://id.twitch.tv/oauth2/authorize?response_type=code&client_id=" + TwitchPlugin.Proxy.twitchApi.Settings.ClientId  + $"&redirect_uri={this.RedirectUrl}" + "&scope=" + String.Join("+", Scopes);

            try
            {
                if (Helpers.IsWindows())
                {
                    using (var p = new System.Diagnostics.Process())
                    {
                        p.StartInfo.FileName = oauthUrl;
                        p.StartInfo.UseShellExecute = true;
                        p.Start();
                    }
                }
                else
                {
                    System.Diagnostics.Process.Start("open", oauthUrl);
                }
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e, $"Twitch AuthenticationServer Authenticate error opening browser: {e.Message}");
                return false;
            }

            return true;
        }

        public void Dispose() => ((IDisposable)this._listener)?.Dispose();
        private void WriteHttpResponse(HttpListenerContext context, String responseString)
        {
            try
            {
                HttpListenerResponse response = context.Response;
                var buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch (Exception ex)
            {
                TwitchPlugin.PluginLog.Error(ex, "Cannot send response string");
            }

        }
        private async void ListenerCallback(IAsyncResult result)
        {
            TwitchPlugin.PluginLog.Info("Twitch AuthenticationServer ListenerCallback");

            try
            {
                if (!this._listener.IsListening)
                {
                    TwitchPlugin.PluginLog.Warning("Go to ListenerCallback but Listener is not listening ?");
                    this.OnTokenError.BeginInvoke(this, new AccessTokenErrorEventArgs(AccessTokenErrorEventArgs.TokenError.ERROR_MISC));
                    return;
                }

                var finalResponseString = AuthenticationServer.ResponseString;
                var context = this._listener.EndGetContext(result);

                var code = context.Request.QueryString["code"];
                
                //FIXME !! For whatever reason, whenever GetAccessTokenFromCodeAsync is used, the response ('success') can no longer be written!
                //var response = await TwitchPlugin.Proxy.twitchApi.Auth.GetAccessTokenFromCodeAsync(code, this._clientSecret, this._redirectUrl);

                var client = new HttpClient();

                var tokenResponse = await client.PostAsync(new Uri("https://id.twitch.tv/oauth2/token?"),
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<String, String>("client_id", TwitchPlugin.Proxy.twitchApi.Settings.ClientId),
                        new KeyValuePair<String, String>("client_secret", TwitchPlugin.Proxy.twitchApi.Settings.Secret),
                        new KeyValuePair<String, String>("grant_type", "authorization_code"),
                        new KeyValuePair<String, String>("redirect_uri", this.RedirectUrl),
                        new KeyValuePair<String, String>("code", code)
                    }));
  
                if (tokenResponse.StatusCode == HttpStatusCode.OK)
                {
                    var rawtext = await tokenResponse.Content.ReadAsStringAsync();
                    var authResp = JsonConvert.DeserializeObject<AuthCodeResponse>(rawtext);

                    if (TwitchProxy.ValidateAccessToken(authResp.AccessToken, out var validationResp))
                    {
                        //Note: Unless we are completely paranoid, we won't validate the scope here
                        this.OnTokenReceived?.BeginInvoke(this, new AccessTokenReceivedEventArgs(authResp.AccessToken, authResp.RefreshToken, validationResp));
                    }
                    else
                    {
                        TwitchPlugin.PluginLog.Warning("Token validation failed, ");
                        finalResponseString = this.GetAuthErrorPage("Token validation failed");
                        this.OnTokenError?.BeginInvoke(this, new AccessTokenErrorEventArgs(AccessTokenErrorEventArgs.TokenError.ERROR_VALIDATION));
                    }
                }
                else
                {
                    TwitchPlugin.PluginLog.Warning("Cannot get access token, possibly invalid client secret");
                    finalResponseString = this.GetAuthErrorPage("Invalid secret");
                    this.OnTokenError?.BeginInvoke(this, new AccessTokenErrorEventArgs(AccessTokenErrorEventArgs.TokenError.ERROR_SECRET));
                }

                { 
                    HttpListenerResponse response = context.Response;
                    byte[] buffer = Encoding.UTF8.GetBytes(finalResponseString);
                    response.ContentLength64 = buffer.Length;
                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                    //this.TokenReceived?.Invoke(this, response /*JsonConvert.DeserializeObject<AuthCodeResponse>(response)*/);
                    //await writer.WriteAsync(finalResponseString);
                }
                TwitchPlugin.PluginLog.Info("Auth successful, existing callback");
                
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e, $"Twitch AuthenticationServer callback error: {e.Message}");
                this.OnTokenError.BeginInvoke(this, new AccessTokenErrorEventArgs(AccessTokenErrorEventArgs.TokenError.ERROR_MISC));
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
