namespace TwitchLib.Api.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Enums;
    using Exceptions;
    using Interfaces;
    using Models;
    using Models.Root;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class ApiBase
    {
        internal const String BaseV5 = "https://api.twitch.tv/kraken";
        internal const String BaseHelix = "https://api.twitch.tv/helix";
        internal const String BaseOauthToken = "https://id.twitch.tv/oauth2/token";
        private readonly IHttpCallHandler _http;
        private readonly TwitchLibJsonSerializer _jsonSerializer;
        private readonly IRateLimiter _rateLimiter;

        private readonly JsonSerializerSettings _twitchLibJsonDeserializer = new JsonSerializerSettings
            {NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore};

        protected readonly IApiSettings Settings;

        public ApiBase(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http)
        {
            this.Settings = settings;
            this._rateLimiter = rateLimiter;
            this._http = http;
            this._jsonSerializer = new TwitchLibJsonSerializer();
        }

        /// <summary>
        ///     Checks the ClientId and AccessToken against the Twitch Api Endpoints
        /// </summary>
        /// <returns>CredentialCheckResponseModel with a success boolean and message</returns>
        public Task<CredentialCheckResponseModel> CheckCredentialsAsync()
        {
            var message = "Check successful";
            var failMessage = "";
            var result = true;
            if (!String.IsNullOrWhiteSpace(this.Settings.ClientId) && !this.ValidClientId(this.Settings.ClientId))
            {
                result = false;
                failMessage =
                    "The passed Client Id was not valid. To get a valid Client Id, register an application here: https://www.twitch.tv/kraken/oauth2/clients/new";
            }

            if (!String.IsNullOrWhiteSpace(this.Settings.AccessToken) &&
                this.ValidAccessToken(this.Settings.AccessToken) == null)
            {
                result = false;
                failMessage +=
                    "The passed Access Token was not valid. To get an access token, go here:  https://twitchtokengenerator.com/";
            }

            return Task.FromResult(new CredentialCheckResponseModel
                {Result = result, ResultMessage = result ? message : failMessage});
        }

        public void DynamicScopeValidation(AuthScopes requiredScope, String accessToken = null)
        {
            //Skip validation if skip is set or access token is null
            if (this.Settings.SkipDynamicScopeValidation || String.IsNullOrWhiteSpace(accessToken)) return;

            //set the scopes based on access token
            this.Settings.Scopes = this.ValidAccessToken(accessToken);
            //skip if no scopes
            if (this.Settings.Scopes == null)
                throw new InvalidCredentialException(
                    $"The current access token does not support this call. Missing required scope: {requiredScope.ToString().ToLower()}. You can skip this check by using: IApiSettings.SkipDynamicScopeValidation = true . You can also generate a new token with this scope here: https://twitchtokengenerator.com");

            if (!this.Settings.Scopes.Contains(requiredScope) && requiredScope != AuthScopes.Any ||
                requiredScope == AuthScopes.Any && this.Settings.Scopes.Any(x => x == AuthScopes.None))
                throw new InvalidCredentialException(
                    $"The current access token ({String.Join(",", this.Settings.Scopes)}) does not support this call. Missing required scope: {requiredScope.ToString().ToLower()}. You can skip this check by using: IApiSettings.SkipDynamicScopeValidation = true . You can also generate a new token with this scope here: https://twitchtokengenerator.com");
        }

        internal virtual Task<Root> GetRootAsync(String authToken = null, String clientId = null)
        {
            return this.TwitchGetGenericAsync<Root>("", ApiVersion.V5, accessToken: authToken, clientId: clientId);
        }

        public String GetAccessToken(String accessToken = null)
        {
            if (!String.IsNullOrEmpty(accessToken))
                return accessToken;
            if (!String.IsNullOrEmpty(this.Settings.AccessToken))
                return this.Settings.AccessToken;
            if (!String.IsNullOrEmpty(this.Settings.Secret) && !String.IsNullOrEmpty(this.Settings.ClientId) &&
                !this.Settings.SkipAutoServerTokenGeneration)
                return this.GenerateServerBasedAccessToken();

            return null;
        }

        internal String GenerateServerBasedAccessToken()
        {
            var result = this._http.GeneralRequest(
                $"{BaseOauthToken}?client_id={this.Settings.ClientId}&client_secret={this.Settings.Secret}&grant_type=client_credentials",
                "POST", null, ApiVersion.Helix, this.Settings.ClientId);
            if (result.Key == 200)
            {
                var user = JsonConvert.DeserializeObject<dynamic>(result.Value);
                var offset = (Int32) user.expires_in;
                return (String) user.access_token;
            }

            return null;
        }

        protected Task<T> TwitchGetGenericAsync<T>(String resource, ApiVersion api,
            List<KeyValuePair<String, String>> getParams = null, String accessToken = null, String clientId = null,
            String customBase = null)
        {
            var url = this.ConstructResourceUrl(resource, getParams, api, customBase);

            if (String.IsNullOrEmpty(clientId) && !String.IsNullOrEmpty(this.Settings.ClientId))
                clientId = this.Settings.ClientId;

            accessToken = this.GetAccessToken(accessToken);

            if (String.IsNullOrEmpty(accessToken))
            {
                return null;
            }
            return this._rateLimiter.Perform(async () =>
                await Task.Run(() =>
                    JsonConvert.DeserializeObject<T>(
                        this._http.GeneralRequest(url, "GET", null, api, clientId, accessToken).Value,
                        this._twitchLibJsonDeserializer)).ConfigureAwait(false));
        }

        protected Task<String> TwitchDeleteAsync(String resource, ApiVersion api,
            List<KeyValuePair<String, String>> getParams = null, String accessToken = null, String clientId = null,
            String customBase = null)
        {
            var url = this.ConstructResourceUrl(resource, getParams, api, customBase);

            if (String.IsNullOrEmpty(clientId) && !String.IsNullOrEmpty(this.Settings.ClientId))
                clientId = this.Settings.ClientId;

            accessToken = this.GetAccessToken(accessToken);

            return this._rateLimiter.Perform(async () =>
                await Task.Run(() => this._http.GeneralRequest(url, "DELETE", null, api, clientId, accessToken).Value)
                    .ConfigureAwait(false));
        }

        protected Task<T> TwitchPostGenericAsync<T>(String resource, ApiVersion api, String payload,
            List<KeyValuePair<String, String>> getParams = null, String accessToken = null, String clientId = null,
            String customBase = null)
        {
            var url = this.ConstructResourceUrl(resource, getParams, api, customBase);

            if (String.IsNullOrEmpty(clientId) && !String.IsNullOrEmpty(this.Settings.ClientId))
                clientId = this.Settings.ClientId;

            accessToken = this.GetAccessToken(accessToken);

            return this._rateLimiter.Perform(async () =>
                await Task.Run(() =>
                    JsonConvert.DeserializeObject<T>(
                        this._http.GeneralRequest(url, "POST", payload, api, clientId, accessToken).Value,
                        this._twitchLibJsonDeserializer)).ConfigureAwait(false));
        }

        protected Task<T> TwitchPostGenericModelAsync<T>(String resource, ApiVersion api, RequestModel model,
            String accessToken = null, String clientId = null, String customBase = null)
        {
            var url = this.ConstructResourceUrl(resource, api: api, overrideUrl: customBase);

            if (String.IsNullOrEmpty(clientId) && !String.IsNullOrEmpty(this.Settings.ClientId))
                clientId = this.Settings.ClientId;

            accessToken = this.GetAccessToken(accessToken);

            return this._rateLimiter.Perform(async () => await Task.Run(() =>
                JsonConvert.DeserializeObject<T>(
                    this._http.GeneralRequest(url, "POST",
                            model != null ? this._jsonSerializer.SerializeObject(model) : "", api, clientId,
                            accessToken)
                        .Value, this._twitchLibJsonDeserializer)).ConfigureAwait(false));
        }

        protected Task<T> TwitchDeleteGenericAsync<T>(String resource, ApiVersion api, String accessToken = null,
            String clientId = null, String customBase = null)
        {
            var url = this.ConstructResourceUrl(resource, null, api, customBase);

            if (String.IsNullOrEmpty(clientId) && !String.IsNullOrEmpty(this.Settings.ClientId))
                clientId = this.Settings.ClientId;

            accessToken = this.GetAccessToken(accessToken);

            return this._rateLimiter.Perform(async () =>
                await Task.Run(() =>
                    JsonConvert.DeserializeObject<T>(
                        this._http.GeneralRequest(url, "DELETE", null, api, clientId, accessToken).Value,
                        this._twitchLibJsonDeserializer)).ConfigureAwait(false));
        }

        protected Task<T> TwitchPutGenericAsync<T>(String resource, ApiVersion api, String payload,
            List<KeyValuePair<String, String>> getParams = null, String accessToken = null, String clientId = null,
            String customBase = null)
        {
            var url = this.ConstructResourceUrl(resource, getParams, api, customBase);

            if (String.IsNullOrEmpty(clientId) && !String.IsNullOrEmpty(this.Settings.ClientId))
                clientId = this.Settings.ClientId;

            accessToken = this.GetAccessToken(accessToken);

            return this._rateLimiter.Perform(async () =>
                await Task.Run(() =>
                    JsonConvert.DeserializeObject<T>(
                        this._http.GeneralRequest(url, "PUT", payload, api, clientId, accessToken).Value,
                        this._twitchLibJsonDeserializer)).ConfigureAwait(false));
        }

        protected Task<String> TwitchPutAsync(String resource, ApiVersion api, String payload,
            List<KeyValuePair<String, String>> getParams = null, String accessToken = null, String clientId = null,
            String customBase = null)
        {
            var url = this.ConstructResourceUrl(resource, getParams, api, customBase);

            if (String.IsNullOrEmpty(clientId) && !String.IsNullOrEmpty(this.Settings.ClientId))
                clientId = this.Settings.ClientId;

            accessToken = this.GetAccessToken(accessToken);

            return this._rateLimiter.Perform(async () =>
                await Task.Run(() => this._http.GeneralRequest(url, "PUT", payload, api, clientId, accessToken).Value)
                    .ConfigureAwait(false));
        }

        protected Task<KeyValuePair<Int32, String>> TwitchPostAsync(String resource, ApiVersion api, String payload,
            List<KeyValuePair<String, String>> getParams = null, String accessToken = null, String clientId = null,
            String customBase = null)
        {
            var url = this.ConstructResourceUrl(resource, getParams, api, customBase);

            if (String.IsNullOrEmpty(clientId) && !String.IsNullOrEmpty(this.Settings.ClientId))
                clientId = this.Settings.ClientId;

            accessToken = this.GetAccessToken(accessToken);

            return this._rateLimiter.Perform(async () =>
                await Task.Run(() => this._http.GeneralRequest(url, "POST", payload, api, clientId, accessToken))
                    .ConfigureAwait(false));
        }


        protected void PutBytes(String url, Byte[] payload)
        {
            this._http.PutBytes(url, payload);
        }

        internal Int32 RequestReturnResponseCode(String url, String method,
            List<KeyValuePair<String, String>> getParams = null)
        {
            return this._http.RequestReturnResponseCode(url, method, getParams);
        }

        protected Task<T> GetGenericAsync<T>(String url, List<KeyValuePair<String, String>> getParams = null,
            String accessToken = null, ApiVersion api = ApiVersion.V5, String clientId = null)
        {
            if (getParams != null)
                for (var i = 0; i < getParams.Count; i++)
                    if (i == 0)
                        url += $"?{getParams[i].Key}={Uri.EscapeDataString(getParams[i].Value)}";
                    else
                        url += $"&{getParams[i].Key}={Uri.EscapeDataString(getParams[i].Value)}";

            if (String.IsNullOrEmpty(clientId) && !String.IsNullOrEmpty(this.Settings.ClientId))
                clientId = this.Settings.ClientId;

            accessToken = this.GetAccessToken(accessToken);

            return this._rateLimiter.Perform(async () =>
                await Task.Run(() =>
                    JsonConvert.DeserializeObject<T>(
                        this._http.GeneralRequest(url, "GET", null, api, clientId, accessToken).Value,
                        this._twitchLibJsonDeserializer)).ConfigureAwait(false));
        }

        internal Task<T> GetSimpleGenericAsync<T>(String url, List<KeyValuePair<String, String>> getParams = null)
        {
            if (getParams != null)
                for (var i = 0; i < getParams.Count; i++)
                    if (i == 0)
                        url += $"?{getParams[i].Key}={Uri.EscapeDataString(getParams[i].Value)}";
                    else
                        url += $"&{getParams[i].Key}={Uri.EscapeDataString(getParams[i].Value)}";
            return this._rateLimiter.Perform(async () =>
                JsonConvert.DeserializeObject<T>(await this.SimpleRequestAsync(url), this._twitchLibJsonDeserializer));
        }

        // credit: https://stackoverflow.com/questions/14290988/populate-and-return-entities-from-downloadstringcompleted-handler-in-windows-pho
        private Task<String> SimpleRequestAsync(String url)
        {
            var tcs = new TaskCompletionSource<String>();
            var client = new WebClient();

            client.DownloadStringCompleted += DownloadStringCompletedEventHandler;
            client.DownloadString(new Uri(url));

            return tcs.Task;

            // local function
            void DownloadStringCompletedEventHandler(Object sender, DownloadStringCompletedEventArgs args)
            {
                if (args.Cancelled)
                    tcs.SetCanceled();
                else if (args.Error != null)
                    tcs.SetException(args.Error);
                else
                    tcs.SetResult(args.Result);

                client.DownloadStringCompleted -= DownloadStringCompletedEventHandler;
            }
        }

        private Boolean ValidClientId(String clientId)
        {
            try
            {
                var result = this.GetRootAsync(null, clientId).GetAwaiter().GetResult();
                return result.Token != null;
            }
            catch (BadRequestException)
            {
                return false;
            }
        }

        private List<AuthScopes> ValidAccessToken(String accessToken)
        {
            try
            {
                var resp = this.GetRootAsync(accessToken).GetAwaiter().GetResult();
                if (resp.Token == null) return null;

                return BuildScopesList(resp.Token);
            }
            catch
            {
                return null;
            }
        }

        private static List<AuthScopes> BuildScopesList(RootToken token)
        {
            var scopes = new List<AuthScopes>();
            foreach (var scope in token.Auth.Scopes)
                switch (scope)
                {
                    case "channel_check_subscription":
                        scopes.Add(AuthScopes.Channel_Check_Subscription);
                        break;
                    case "channel_commercial":
                        scopes.Add(AuthScopes.Channel_Commercial);
                        break;
                    case "channel_editor":
                        scopes.Add(AuthScopes.Channel_Editor);
                        break;
                    case "channel_feed_edit":
                        scopes.Add(AuthScopes.Channel_Feed_Edit);
                        break;
                    case "channel_feed_read":
                        scopes.Add(AuthScopes.Channel_Feed_Read);
                        break;
                    case "channel_read":
                        scopes.Add(AuthScopes.Channel_Read);
                        break;
                    case "channel_stream":
                        scopes.Add(AuthScopes.Channel_Stream);
                        break;
                    case "channel_subscriptions":
                        scopes.Add(AuthScopes.Channel_Subscriptions);
                        break;
                    case "chat_login":
                        scopes.Add(AuthScopes.Chat_Login);
                        break;
                    case "collections_edit":
                        scopes.Add(AuthScopes.Collections_Edit);
                        break;
                    case "communities_edit":
                        scopes.Add(AuthScopes.Communities_Edit);
                        break;
                    case "communities_moderate":
                        scopes.Add(AuthScopes.Communities_Moderate);
                        break;
                    case "user_blocks_edit":
                        scopes.Add(AuthScopes.User_Blocks_Edit);
                        break;
                    case "user_blocks_read":
                        scopes.Add(AuthScopes.User_Blocks_Read);
                        break;
                    case "user_follows_edit":
                        scopes.Add(AuthScopes.User_Follows_Edit);
                        break;
                    case "user_read":
                        scopes.Add(AuthScopes.User_Read);
                        break;
                    case "user_subscriptions":
                        scopes.Add(AuthScopes.User_Subscriptions);
                        break;
                    case "openid":
                        scopes.Add(AuthScopes.OpenId);
                        break;
                    case "viewing_activity_read":
                        scopes.Add(AuthScopes.Viewing_Activity_Read);
                        break;
                    case "user:edit":
                        scopes.Add(AuthScopes.Helix_User_Edit);
                        break;
                    case "user:edit:broadcast":
                        scopes.Add(AuthScopes.Helix_User_Edit_Broadcast);
                        break;
                    case "user:read:broadcast":
                        scopes.Add(AuthScopes.Helix_User_Read_Broadcast);
                        break;
                    case "user:read:email":
                        scopes.Add(AuthScopes.Helix_User_Read_Email);
                        break;
                    case "clips:edit":
                        scopes.Add(AuthScopes.Helix_Clips_Edit);
                        break;
                    case "bits:read":
                        scopes.Add(AuthScopes.Helix_Bits_Read);
                        break;
                    case "analytics:read:games":
                        scopes.Add(AuthScopes.Helix_Analytics_Read_Games);
                        break;
                    case "analytics:read:extensions":
                        scopes.Add(AuthScopes.Helix_Analytics_Read_Extensions);
                        break;
                }

            if (scopes.Count == 0)
                scopes.Add(AuthScopes.None);
            return scopes;
        }

        private String ConstructResourceUrl(String resource = null, List<KeyValuePair<String, String>> getParams = null,
            ApiVersion api = ApiVersion.V5, String overrideUrl = null)
        {
            var url = "";
            if (overrideUrl == null)
            {
                if (resource == null)
                    throw new Exception("Cannot pass null resource with null override url");
                switch (api)
                {
                    case ApiVersion.V5:
                        url = $"{BaseV5}{resource}";
                        break;
                    case ApiVersion.Helix:
                        url = $"{BaseHelix}{resource}";
                        break;
                }
            }
            else
            {
                url = resource == null ? overrideUrl : $"{overrideUrl}{resource}";
            }

            if (getParams != null)
                for (var i = 0; i < getParams.Count; i++)
                    if (i == 0)
                        url += $"?{getParams[i].Key}={Uri.EscapeDataString(getParams[i].Value)}";
                    else
                        url += $"&{getParams[i].Key}={Uri.EscapeDataString(getParams[i].Value)}";
            return url;
        }

        private class TwitchLibJsonSerializer
        {
            private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
            {
                ContractResolver = new LowercaseContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            public String SerializeObject(Object o)
            {
                return JsonConvert.SerializeObject(o, Formatting.Indented, this._settings);
            }

            private class LowercaseContractResolver : DefaultContractResolver
            {
                protected override String ResolvePropertyName(String propertyName)
                {
                    return propertyName.ToLower();
                }
            }
        }
    }
}