namespace TwitchLib.Api.Helix
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core;
    using Core.Enums;
    using Core.Interfaces;
    using Models.Analytics;

    public class Analytics : ApiBase
    {
        public Analytics(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings, rateLimiter, http)
        {
        }

        #region GetGameAnalytics

        public Task<GetGameAnalyticsResponse> GetGameAnalyticsAsync(String gameId = null, String authToken = null)
        {
            this.DynamicScopeValidation(AuthScopes.Helix_Analytics_Read_Games, authToken);
            var getParams = new List<KeyValuePair<String, String>>();
            if (gameId != null)
                getParams.Add(new KeyValuePair<String, String>("game_id", gameId));

            return this.TwitchGetGenericAsync<GetGameAnalyticsResponse>("/analytics/games", ApiVersion.Helix, getParams, authToken);
        }

        #endregion

        #region GetExtensionAnalytics

        public Task<GetExtensionAnalyticsResponse> GetExtensionAnalyticsAsync(String extensionId, String authToken = null)
        {
            this.DynamicScopeValidation(AuthScopes.Helix_Analytics_Read_Extensions, authToken);
            var getParams = new List<KeyValuePair<String, String>>();
            if (extensionId != null)
                getParams.Add(new KeyValuePair<String, String>("extension_id", extensionId));

            return this.TwitchGetGenericAsync<GetExtensionAnalyticsResponse>("/analytics/extensions", ApiVersion.Helix, getParams, authToken);
        }

        #endregion

    }
}