namespace TwitchLib.Api.Helix
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core;
    using Core.Enums;
    using Core.Extensions.System;
    using Core.Interfaces;
    using Models.Bits;

    public class Bits :ApiBase
    {
        public Bits(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings, rateLimiter, http)
        {
        }

        #region GetBitsLeaderboard

        public Task<GetBitsLeaderboardResponse> GetBitsLeaderboardAsync(Int32 count = 10, BitsLeaderboardPeriodEnum period = BitsLeaderboardPeriodEnum.All, DateTime? startedAt = null, String userid = null, String accessToken = null)
        {
            this.DynamicScopeValidation(AuthScopes.Helix_Bits_Read, accessToken);

            var getParams = new List<KeyValuePair<String, String>>
                    {
                        new KeyValuePair<String, String>("count", count.ToString())
                    };

            switch (period)
            {
                case BitsLeaderboardPeriodEnum.Day:
                    getParams.Add(new KeyValuePair<String, String>("period", "day"));
                    break;
                case BitsLeaderboardPeriodEnum.Week:
                    getParams.Add(new KeyValuePair<String, String>("period", "week"));
                    break;
                case BitsLeaderboardPeriodEnum.Month:
                    getParams.Add(new KeyValuePair<String, String>("period", "month"));
                    break;
                case BitsLeaderboardPeriodEnum.Year:
                    getParams.Add(new KeyValuePair<String, String>("period", "year"));
                    break;
                case BitsLeaderboardPeriodEnum.All:
                    getParams.Add(new KeyValuePair<String, String>("period", "all"));
                    break;
            }

            if (startedAt != null)
                getParams.Add(new KeyValuePair<String, String>("started_at", startedAt.Value.ToRfc3339String()));
            if (userid != null)
                getParams.Add(new KeyValuePair<String, String>("user_id", userid));

            return this.TwitchGetGenericAsync<GetBitsLeaderboardResponse>("/bits/leaderboard", ApiVersion.Helix, getParams);
        }

        #endregion
    }
}