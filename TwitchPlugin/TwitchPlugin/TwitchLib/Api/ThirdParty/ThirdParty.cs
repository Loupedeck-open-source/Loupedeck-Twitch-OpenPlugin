namespace TwitchLib.Api.ThirdParty
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using System.Timers;
    using AuthorizationFlow;
    using Core;
    using Core.Enums;
    using Core.Interfaces;
    using Events;
    using ModLookup;
    using Newtonsoft.Json;
    using UsernameChange;

    /// <summary>These endpoints are offered by third party services (NOT TWITCH), but are still pretty cool.</summary>
    public class ThirdParty
    {
        public ThirdParty(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http)
        {
            this.UsernameChange = new UsernameChangeApi(settings, rateLimiter, http);
            this.ModLookup = new ModLookupApi(settings, rateLimiter, http);
            this.AuthorizationFlow = new AuthorizationFlowApi(settings, rateLimiter, http);
        }

        public UsernameChangeApi UsernameChange { get; }
        public ModLookupApi ModLookup { get; }
        public AuthorizationFlowApi AuthorizationFlow { get; }

        public class UsernameChangeApi : ApiBase
        {
            public UsernameChangeApi(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings, rateLimiter, http)
            {
            }

            #region GetUsernameChanges

            public Task<List<UsernameChangeListing>> GetUsernameChangesAsync(String username)
            {
                var getParams = new List<KeyValuePair<String, String>>
                {
                    new KeyValuePair<String, String>("q", username),
                    new KeyValuePair<String, String>("format", "json")
                };

                return this.GetGenericAsync<List<UsernameChangeListing>>("https://twitch-tools.rootonline.de/username_changelogs_search.php", getParams, null, ApiVersion.Void);
            }

            #endregion
        }

        public class ModLookupApi : ApiBase
        {
            public ModLookupApi(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings, rateLimiter, http)
            {
            }

            public Task<ModLookupResponse> GetChannelsModdedForByNameAsync(String username, Int32 offset = 0, Int32 limit = 100, Boolean useTls12 = true)
            {
                if (useTls12)
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var getParams = new List<KeyValuePair<String, String>>
                {
                    new KeyValuePair<String, String>("offset", offset.ToString()),
                    new KeyValuePair<String, String>("limit", limit.ToString())
                };
                return this.GetGenericAsync<ModLookupResponse>($"https://twitchstuff.3v.fi/modlookup/api/user/{username}", getParams, null, ApiVersion.Void);
            }

            public Task<TopResponse> GetChannelsModdedForByTopAsync(Boolean useTls12 = true)
            {
                if (useTls12)
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                return this.GetGenericAsync<TopResponse>("https://twitchstuff.3v.fi/modlookup/api/top");
            }

            public Task<StatsResponse> GetChannelsModdedForStatsAsync(Boolean useTls12 = true)
            {
                if (useTls12)
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                return this.GetGenericAsync<StatsResponse>("https://twitchstuff.3v.fi/modlookup/api/stats");
            }
        }

        public class AuthorizationFlowApi : ApiBase
        {
            private const String BaseUrl = "https://twitchtokengenerator.com/api";
            private String _apiId;
            private Timer _pingTimer;

            public AuthorizationFlowApi(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings, rateLimiter, http)
            {
            }

            public event EventHandler<OnUserAuthorizationDetectedArgs> OnUserAuthorizationDetected;
            public event EventHandler<OnAuthorizationFlowErrorArgs> OnError;

            public CreatedFlow CreateFlow(String applicationTitle, IEnumerable<AuthScopes> scopes)
            {
                String scopesStr = null;
                foreach (var scope in scopes)
                    if (scopesStr == null)
                        scopesStr = Core.Common.Helpers.AuthScopesToString(scope);
                    else
                        scopesStr += $"+{Core.Common.Helpers.AuthScopesToString(scope)}";

                var createUrl = $"{BaseUrl}/create/{Core.Common.Helpers.Base64Encode(applicationTitle)}/{scopesStr}";

                var resp = new WebClient().DownloadString(createUrl);
                return JsonConvert.DeserializeObject<CreatedFlow>(resp);
            }

            public RefreshTokenResponse RefreshToken(String refreshToken)
            {
                var refreshUrl = $"{BaseUrl}/refresh/{refreshToken}";

                var resp = new WebClient().DownloadString(refreshUrl);
                return JsonConvert.DeserializeObject<RefreshTokenResponse>(resp);
            }

            public void BeginPingingStatus(String id, Int32 intervalMs = 5000)
            {
                this._apiId = id;
                this._pingTimer = new Timer(intervalMs);
                this._pingTimer.Elapsed += this.OnPingTimerElapsed;
                this._pingTimer.Start();
            }

            public PingResponse PingStatus(String id = null)
            {
                if (id != null)
                    this._apiId = id;

                var resp = new WebClient().DownloadString($"{BaseUrl}/status/{this._apiId}");
                var model = new PingResponse(resp);

                return model;
            }

            private void OnPingTimerElapsed(Object sender, ElapsedEventArgs e)
            {
                var ping = this.PingStatus();
                if (ping.Success)
                {
                    this._pingTimer.Stop();
                    this.OnUserAuthorizationDetected?.Invoke(null, new OnUserAuthorizationDetectedArgs {Id = ping.Id, Scopes = ping.Scopes, Token = ping.Token, Username = ping.Username, Refresh = ping.Refresh});
                }
                else
                {
                    if (ping.Error == 3) return;

                    this._pingTimer.Stop();
                    this.OnError?.Invoke(null, new OnAuthorizationFlowErrorArgs {Error = ping.Error, Message = ping.Message});
                }
            }
        }
    }
}