namespace TwitchLib.Api.Helix
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core;
    using Core.Enums;
    using Core.Interfaces;
    using Loupedeck.TwitchPlugin;
    using Models.Streams;
    using Models.StreamsMetadata;

    public class Streams : ApiBase
    {
        public Streams(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings, rateLimiter, http)
        {
        }

        public Task<CreateMarkerResponse> CreateMarkerAsync(String userId, String description = null)
        {
            var parameters = new List<KeyValuePair<String, String>>
            {
                new KeyValuePair<String, String>("user_id", userId),
                new KeyValuePair<String, String>("description", description)
            };

            return this.TwitchPostGenericAsync<CreateMarkerResponse>("/streams/markers", ApiVersion.Helix, null, parameters);
        }

        public Task<GetStreamsResponse> GetStreamsAsync(String after = null, List<String> communityIds = null, Int32 first = 20, List<String> gameIds = null, List<String> languages = null, String type = "all", List<String> userIds = null, List<String> userLogins = null)
        {
            var getParams = new List<KeyValuePair<String, String>>
                {
                    new KeyValuePair<String, String>("first", first.ToString()),
                    new KeyValuePair<String, String>("type", type)
                };
            if (after != null)
                getParams.Add(new KeyValuePair<String, String>("after", after));
            if (communityIds != null && communityIds.Count > 0)
            {
                foreach (var communityId in communityIds)
                    getParams.Add(new KeyValuePair<String, String>("community_id", communityId));
            }

            if (gameIds != null && gameIds.Count > 0)
            {
                foreach (var gameId in gameIds)
                    getParams.Add(new KeyValuePair<String, String>("game_id", gameId));
            }

            if (languages != null && languages.Count > 0)
            {
                foreach (var language in languages)
                    getParams.Add(new KeyValuePair<String, String>("language", language));
            }

            if (userIds != null && userIds.Count > 0)
            {
                foreach (var userId in userIds)
                    getParams.Add(new KeyValuePair<String, String>("user_id", userId));
            }

            if (userLogins != null && userLogins.Count > 0)
            {
                foreach (var userLogin in userLogins)
                    getParams.Add(new KeyValuePair<String, String>("user_login", userLogin));
            }

            return this.TwitchGetGenericAsync<GetStreamsResponse>($"/streams", ApiVersion.Helix, getParams);
        }

        public Task<GetStreamsMetadataResponse> GetStreamsMetadataAsync(String after = null, List<String> communityIds = null, Int32 first = 20, List<String> gameIds = null, List<String> languages = null, String type = "all", List<String> userIds = null, List<String> userLogins = null)
        {
            var getParams = new List<KeyValuePair<String, String>>
                {
                    new KeyValuePair<String, String>("first", first.ToString()),
                    new KeyValuePair<String, String>("type", type)
                };
            if (after != null)
                getParams.Add(new KeyValuePair<String, String>("after", after));
            if (communityIds != null && communityIds.Count > 0)
            {
                foreach (var communityId in communityIds)
                    getParams.Add(new KeyValuePair<String, String>("community_id", communityId));
            }

            if (gameIds != null && gameIds.Count > 0)
            {
                foreach (var gameId in gameIds)
                    getParams.Add(new KeyValuePair<String, String>("game_id", gameId));
            }

            if (languages != null && languages.Count > 0)
            {
                foreach (var language in languages)
                    getParams.Add(new KeyValuePair<String, String>("language", language));
            }

            if (userIds != null && userIds.Count > 0)
            {
                foreach (var userId in userIds)
                    getParams.Add(new KeyValuePair<String, String>("user_id", userId));
            }

            if (userLogins != null && userLogins.Count > 0)
            {
                foreach (var userLogin in userLogins)
                    getParams.Add(new KeyValuePair<String, String>("user_login", userLogin));
            }

            return this.TwitchGetGenericAsync<GetStreamsMetadataResponse>("/streams/metadata", ApiVersion.Helix, getParams);
        }
    }

}