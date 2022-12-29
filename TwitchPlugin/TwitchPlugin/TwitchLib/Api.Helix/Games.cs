namespace TwitchLib.Api.Helix
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core;
    using Core.Enums;
    using Core.Exceptions;
    using Core.Interfaces;
    using Models.Games;

    public class Games : ApiBase
        {
            public Games(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings, rateLimiter, http)
            {
            }

            #region GetGames

            public Task<GetGamesResponse> GetGamesAsync(List<String> gameIds = null, List<String> gameNames = null)
            {
                if (gameIds == null && gameNames == null || gameIds != null && gameIds.Count == 0 && gameNames == null || gameNames != null && gameNames.Count == 0 && gameIds == null)
                    throw new BadParameterException("Either gameIds or gameNames must have at least one value");

                if (gameIds != null && gameIds.Count > 100)
                    throw new BadParameterException("gameIds list cannot exceed 100 items");

                if (gameNames != null && gameNames.Count > 100)
                    throw new BadParameterException("gameNames list cannot exceed 100 items");

                var getParams = new List<KeyValuePair<String, String>>();
                if (gameIds != null && gameIds.Count > 0)
                {
                    foreach (var gameId in gameIds)
                        getParams.Add(new KeyValuePair<String, String>("id", gameId));
                }

                if (gameNames != null && gameNames.Count > 0)
                {
                    foreach (var gameName in gameNames)
                        getParams.Add(new KeyValuePair<String, String>("name", gameName));
                }

                return this.TwitchGetGenericAsync<GetGamesResponse>("/games", ApiVersion.Helix, getParams);
            }

            #endregion

            #region GetTopGames

            public Task<GetTopGamesResponse> GetTopGamesAsync(String before = null, String after = null, Int32 first = 20)
            {
                if (first < 0 || first > 100)
                    throw new BadParameterException("'first' parameter must be between 1 (inclusive) and 100 (inclusive).");

                var getParams = new List<KeyValuePair<String, String>>
                {
                        new KeyValuePair<String, String>("first", first.ToString())
                };

                if (before != null)
                    getParams.Add(new KeyValuePair<String, String>("before", before));
                if (after != null)
                    getParams.Add(new KeyValuePair<String, String>("after", after));

                return this.TwitchGetGenericAsync<GetTopGamesResponse>("/games/top", ApiVersion.Helix, getParams);
            }

            #endregion
        
    }
}