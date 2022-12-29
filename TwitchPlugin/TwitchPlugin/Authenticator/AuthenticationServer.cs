namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;

    public class AuthenticationServer : IAuthenticationServer
    {
        private const String ResponseString = "<html><head><meta http-equiv=\"refresh\" content=\"0;URL='https://login.loupedeck.com/static-pages/signin-success.html?service=twitch'\" /></head><body>Authenticated</body></html>";

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

        private HttpListener _listener;
        private String _redirectUrl;
        private String _clientId;
        private String _clientSecret;

        public EventHandler<Token> TokenReceived { get; set; }


        public void Start(TwitchPluginConfig config)
        {
            TwitchPlugin.PluginLog.Info("Twitch AuthenticationServer Start");

            try
            {
                if (this._listener != null)
                {
                    TwitchPlugin.PluginLog.Info("Twitch AuthenticationServer: Listener already started.");
                    return;
                }

                if (!NetworkHelpers.TryGetFreeTcpPort(config.Ports, out var port))
                {
                    TwitchPlugin.PluginLog.Error("Twitch AuthenticationServer: No available ports for Twitch!");
                    return;
                }

                this._redirectUrl = $"http://localhost:{port}/";
                this._clientId = config.ClientId;
                this._clientSecret = config.ClientSecret;

                this._listener = new HttpListener();
                this._listener.Prefixes.Add(this._redirectUrl);
                this._listener.Start();
                this._listener.BeginGetContext(this.ListenerCallback, this._listener);
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e,$"Twitch AuthenticationServer Start error: {e.Message}");
            }
        }

        public void Authenticate()
        {
            TwitchPlugin.PluginLog.Info("Twitch AuthenticationServer Authenticate");

            if (!this._listener.IsListening)
            {
                TwitchPlugin.PluginLog.Error("Twitch AuthenticationServer: Listener has not been started yet.");
                return;
            }

            var oauthUrl = "https://id.twitch.tv/oauth2/authorize?response_type=code&client_id=" +
                           this._clientId + $"&redirect_uri={this._redirectUrl}" + "&scope=" + String.Join("+", Scopes);

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
                TwitchPlugin.PluginLog.Error(e,$"Twitch AuthenticationServer Authenticate error opening browser: {e.Message}");
            }
        }

        public void RefreshAccessToken(String refreshToken)
        {
            TwitchPlugin.PluginLog.Info("Twitch AuthenticationServer RefreshAccessToken");

            var refreshRequest = (HttpWebRequest)WebRequest.Create("https://id.twitch.tv/oauth2/token" +
                                                                    $"?grant_type=refresh_token&refresh_token={refreshToken}&client_id={this._clientId}&client_secret={this._clientSecret}");
            refreshRequest.Method = "POST";
            try
            {
                var response = refreshRequest.GetResponse();
                var stream = response.GetResponseStream();
                var reader = new StreamReader(stream);
                var responseFromServer = reader.ReadToEnd();
                var token = JsonConvert.DeserializeObject<Token>(responseFromServer);
                if (!this.IsTokenValid(token.AccessToken))
                {
                    throw new InvalidAccessTokenException();
                }
                this.TokenReceived?.Invoke(this, token);
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

        public Boolean IsTokenValid(String accessToken)
        {
            try
            {
                var tokenInfo = TwitchHelpers.GetTokenInfo(accessToken);
                return Scopes.Intersect(tokenInfo.Scopes).Count() == Scopes.Count;
            }
            catch (InvalidAccessTokenException)
            {
                TwitchPlugin.PluginLog.Info("Twitch AuthenticationServer: Invalid access token.");
                return false;
            }
        }

        public void Dispose()
        {
            ((IDisposable)this._listener)?.Dispose();
        }

        private async void ListenerCallback(IAsyncResult result)
        {
            TwitchPlugin.PluginLog.Info("Twitch AuthenticationServer ListenerCallback");

            try
            {
                var listener = (HttpListener)result.AsyncState;
                if (!listener.IsListening)
                {
                    return;
                }

                var context = listener.EndGetContext(result);
                var code = context.Request.QueryString["code"];
                using (var client = new HttpClient())
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
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
                    this.TokenReceived?.Invoke(this, JsonConvert.DeserializeObject<Token>(response));
                    await writer.WriteAsync(ResponseString);
                }
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Error(e,$"Twitch AuthenticationServer callback error: {e.Message}");
            }
            finally
            {
                try
                {
                    if (this._listener.IsListening)
                    {
                        this._listener.BeginGetContext(this.ListenerCallback, this._listener);
                    }
                }
                catch (Exception ex)
                {
                    TwitchPlugin.PluginLog.Error(ex,"Twitch AuthenticationServer callback error in 'finally'");
                }
            }
        }
    }
}