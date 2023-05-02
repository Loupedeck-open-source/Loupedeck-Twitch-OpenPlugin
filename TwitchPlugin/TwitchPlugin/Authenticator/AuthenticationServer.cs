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
    using System.Runtime.Remoting.Contexts;
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
            "user:read:email"
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
                this._listener.BeginGetContext(new AsyncCallback(this.ListenerCallback), this._listener);
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
            /* we are using OAuth Implicit grant flow https://dev.twitch.tv/docs/authentication/getting-tokens-oidc/#oidc-implicit-grant-flow
             * 1 - step authorization: we send all this and then wait for the response where code is sent
             */

            var oauthUrl = "https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=" + TwitchPlugin.Proxy.twitchApi.Settings.ClientId + $"&redirect_uri={this.RedirectUrl}" + "&scope=" + String.Join("+", Scopes);

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
        private void ListenerCallback(IAsyncResult result)
        {
            TwitchPlugin.PluginLog.Info("Twitch AuthenticationServer ListenerCallback");

            if (!this._listener.IsListening)
            {
                TwitchPlugin.PluginLog.Warning("Go to ListenerCallback but Listener is not listening ?");
                this.OnTokenError.BeginInvoke(this, new AccessTokenErrorEventArgs(AccessTokenErrorEventArgs.TokenError.ERROR_MISC));
                return;
            }

            var finalResponseString = AuthenticationServer.ResponseString;
            var context = this._listener.EndGetContext(result);
            var continueListening = false;
            try
            {
                /* this is kind of response we are receiving 
                 http://localhost:3000/
                    #access_token=73gl5dipwta5fsfma3ia05woyffbp
                    &id_token=eyJhbGciOiJSUzI1NiIsInR5cC6IkpXVCIsImtpZCI6IjEifQ...
                    &scope=channel%253Amanage%253Apolls+channel%253Aread%253Apolls+openid
                    &state=c3ab8aa609ea11e793ae92361f002671
                    &token_type=bearer
                 */

                //Access token needs to be extracted from the fragment portion of the URI we receive to the server

                if (context.Request.QueryString.AllKeys.Contains("error"))
                {
                    TwitchPlugin.PluginLog.Warning("Cannot get access token, possibly invalid client secret");
                    finalResponseString = this.GetAuthErrorPage($"Error authenticating - \"{context.Request.QueryString["error"]}\" - \"{context.Request.QueryString["error_description"]}\"");
                    this.OnTokenError?.BeginInvoke(this, new AccessTokenErrorEventArgs(AccessTokenErrorEventArgs.TokenError.ERROR_SECRET));
                }
                else if (!context.Request.QueryString.AllKeys.Contains("access_token"))
                {

                    //The browser is redirectred to localhost:3000/#access_toke=xxxxx   
                    //But we are not receiving it here. We need to send javacript to 
                    TwitchPlugin.PluginLog.Warning("Intial request received, creating redirect to get auth code");
                    finalResponseString = $@"
<html><head><body><script type ='text/javascript'>
const urlParams = new URLSearchParams(window.location.hash.replace('#', '?'));
const token = urlParams.get('access_token');
const state = urlParams.get('state');
if (token && token.length)
    window.location.href = '{context.Request.Url}/redirect/?access_token=' + token + '&state=' + state;
</script></body>";
                    continueListening = true;
                   
                    this._listener.BeginGetContext(new AsyncCallback(this.ListenerCallback), this._listener);
                }
                else if (context.Request.QueryString.AllKeys.Contains("access_token"))
                { 
                    TwitchPlugin.PluginLog.Info($"Got access token: {context.Request.Url.AbsoluteUri}");
                    var frag = context.Request.Url.AbsoluteUri;
                    //Should be like access_token=73gl5dipwta5fsfma3ia05woyffbp
                    var accessToken = frag.Split("&")[0].Split('=')[1];
                    TwitchPlugin.PluginLog.Info($"Got access token: {accessToken}");

                    if (TwitchProxy.ValidateAccessToken(accessToken, out var validationResp))
                    {
                        //Note: Unless we are completely paranoid, we won't validate the scope here
                        this.OnTokenReceived?.BeginInvoke(this, new AccessTokenReceivedEventArgs(accessToken, null, validationResp));
                    }
                    else
                    {
                        TwitchPlugin.PluginLog.Warning("Token validation failed, ");
                        finalResponseString = this.GetAuthErrorPage("Error validating access token");
                        this.OnTokenError?.BeginInvoke(this, new AccessTokenErrorEventArgs(AccessTokenErrorEventArgs.TokenError.ERROR_MISC));
                    }
                }
                else
                {
                    TwitchPlugin.PluginLog.Warning("authserver: Received no parameters. Continue listening");
                    continueListening = true;
                }

            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e, $"Twitch AuthenticationServer callback error: {e.Message}");
                this.OnTokenError.BeginInvoke(this, new AccessTokenErrorEventArgs(AccessTokenErrorEventArgs.TokenError.ERROR_MISC));
            }
            finally
            {
                this.WriteHttpResponse(context, finalResponseString);

                try
                {
                    if (this._listener.IsListening && !continueListening)
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
