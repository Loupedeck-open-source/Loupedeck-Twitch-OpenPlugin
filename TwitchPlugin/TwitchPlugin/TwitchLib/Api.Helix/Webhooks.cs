namespace TwitchLib.Api.Helix
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core;
    using Core.Enums;
    using Core.Exceptions;
    using Core.Interfaces;
    using Models.Webhooks;

    public class Webhooks : ApiBase
    {
        public Webhooks(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings, rateLimiter, http)
        {
        }

        #region GetWebhookSubscriptions
        public Task<GetWebhookSubscriptionsResponse> GetWebhookSubscriptionsAsync(String after = null, Int32 first = 20, String accessToken = null)
        {
            if (first < 1 || first > 100)
                throw new BadParameterException("'first' must between 1 (inclusive) and 100 (inclusive).");

            var getParams = new List<KeyValuePair<String, String>>
                {
                    new KeyValuePair<String, String>("first", first.ToString())
                };
            if (after != null)
                getParams.Add(new KeyValuePair<String, String>("after", after));


            accessToken = this.GetAccessToken(accessToken);

            if (String.IsNullOrEmpty(accessToken))
                throw new BadParameterException("'accessToken' be supplied, set AccessToken in settings or ClientId and Secret in settings");

            return this.TwitchGetGenericAsync<GetWebhookSubscriptionsResponse>("/webhooks/subscriptions", ApiVersion.Helix, getParams, accessToken);
        }

        #endregion

        #region UserFollowsSomeone

        public Task<Boolean> UserFollowsSomeoneAsync(String callbackUrl, WebhookCallMode mode, String userInitiatorId, TimeSpan? duration = null, String signingSecret = null, String accessToken = null)
        {
            var leaseSeconds = (Int32)this.ValidateTimespan(duration).TotalSeconds;

            return this.PerformWebhookRequestAsync(mode, $"https://api.twitch.tv/helix/users/follows?first=1&from_id={userInitiatorId}", callbackUrl, leaseSeconds, signingSecret, accessToken);
        }

        #endregion

        #region UserReceivesFollower

        public Task<Boolean> UserReceivesFollowerAsync(String callbackUrl, WebhookCallMode mode, String userReceiverId, TimeSpan? duration = null, String signingSecret = null, String accessToken = null)
        {
            var leaseSeconds = (Int32)this.ValidateTimespan(duration).TotalSeconds;

            return this.PerformWebhookRequestAsync(mode, $"https://api.twitch.tv/helix/users/follows?first=1&to_id={userReceiverId}", callbackUrl, leaseSeconds, signingSecret, accessToken);
        }

        #endregion

        #region UserFollowsUser

        public Task<Boolean> UserFollowsUserAsync(String callbackUrl, WebhookCallMode mode, String userInitiator, String userReceiverId, TimeSpan? duration = null, String signingSecret = null, String accessToken = null)
        {
            var leaseSeconds = (Int32)this.ValidateTimespan(duration).TotalSeconds;

            return this.PerformWebhookRequestAsync(mode, $"https://api.twitch.tv/helix/users/follows?first=1&from_id={userInitiator}&to_id={userReceiverId}", callbackUrl, leaseSeconds, signingSecret);
        }

        #endregion

        #region StreamUpDown

        public Task<Boolean> StreamUpDownAsync(String callbackUrl, WebhookCallMode mode, String userId, TimeSpan? duration = null, String signingSecret = null, String accessToken = null)
        {
            var leaseSeconds = (Int32)this.ValidateTimespan(duration).TotalSeconds;

            return this.PerformWebhookRequestAsync(mode, $"https://api.twitch.tv/helix/streams?user_id={userId}", callbackUrl, leaseSeconds, signingSecret, accessToken);
        }

        #endregion

        #region UserChanged

        public Task<Boolean> UserChangedAsync(String callbackUrl, WebhookCallMode mode, Int32 id, TimeSpan? duration = null, String signingSecret = null)
        {
            var leaseSeconds = (Int32)this.ValidateTimespan(duration).TotalSeconds;

            return this.PerformWebhookRequestAsync(mode, $"https://api.twitch.tv/helix/users?id={id}", callbackUrl, leaseSeconds, signingSecret);
        }

        #endregion

        #region GameAnalytics

        public Task<Boolean> GameAnalyticsAsync(String callbackUrl, WebhookCallMode mode, String gameId, TimeSpan? duration = null, String signingSecret = null, String authToken = null)
        {
            this.DynamicScopeValidation(AuthScopes.Helix_Analytics_Read_Games, authToken);
            var leaseSeconds = (Int32)this.ValidateTimespan(duration).TotalSeconds;

            return this.PerformWebhookRequestAsync(mode, $"https://api.twitch.tv/helix/analytics/games?game_id={gameId}", callbackUrl, leaseSeconds, signingSecret);
        }

        #endregion

        private TimeSpan ValidateTimespan(TimeSpan? duration)
        {
            if (duration != null && duration.Value > TimeSpan.FromDays(10))
                throw new BadParameterException("Maximum timespan allowed for webhook subscription duration is 10 days.");

            return duration ?? TimeSpan.FromDays(10);
        }

        private async Task<Boolean> PerformWebhookRequestAsync(WebhookCallMode mode, String topicUrl, String callbackUrl, Int32 leaseSeconds, String signingSecret = null, String accessToken = null)
        {
            var getParams = new List<KeyValuePair<String, String>>
                {
                    mode == WebhookCallMode.Subscribe ? new KeyValuePair<String, String>("hub.mode", "subscribe") : new KeyValuePair<String, String>("hub.mode", "unsubscribe"),
                    new KeyValuePair<String, String>("hub.topic", topicUrl),
                    new KeyValuePair<String, String>("hub.callback", callbackUrl),
                    new KeyValuePair<String, String>("hub.lease_seconds", leaseSeconds.ToString())
                };


            if (signingSecret != null)
                getParams.Add(new KeyValuePair<String, String>("hub.secret", signingSecret));
            var resp = await this.TwitchPostAsync("/webhooks/hub", ApiVersion.Helix, null, getParams, accessToken).ConfigureAwait(false);

            return resp.Key == 202;
        }
    }

}