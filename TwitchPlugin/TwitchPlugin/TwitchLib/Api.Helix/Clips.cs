namespace TwitchLib.Api.Helix
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core;
    using Core.Enums;
    using Core.Exceptions;
    using Core.Interfaces;
    using Models.Clips.CreateClip;
    using Models.Clips.GetClip;

    public class Clips : ApiBase
    {
        public Clips(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings, rateLimiter, http)
        { }

        #region GetClip

        public Task<GetClipResponse> GetClipAsync(String clipId = null, String gameId = null, String broadcasterId = null, String before = null, String after = null, Int32 first = 20)
        {
            if (first < 0 || first > 100)
                throw new BadParameterException("'first' must between 0 (inclusive) and 100 (inclusive).");

            var getParams = new List<KeyValuePair<String, String>>();
            if (clipId != null)
                getParams.Add(new KeyValuePair<String, String>("id", clipId));
            if (gameId != null)
                getParams.Add(new KeyValuePair<String, String>("game_id", gameId));
            if (broadcasterId != null)
                getParams.Add(new KeyValuePair<String, String>("broadcaster_id", broadcasterId));

            if (getParams.Count != 1)
                throw new BadParameterException("One of the following parameters must be set: clipId, gameId, broadcasterId. Only one is allowed to be set.");

            if (before != null)
                getParams.Add(new KeyValuePair<String, String>("before", before));
            if (after != null)
                getParams.Add(new KeyValuePair<String, String>("after", after));
            getParams.Add(new KeyValuePair<String, String>("first", first.ToString()));

            return this.TwitchGetGenericAsync<GetClipResponse>("/clips", ApiVersion.Helix, getParams);
        }

        #endregion

        #region CreateClip

        public Task<CreatedClipResponse> CreateClipAsync(String broadcasterId, String authToken = null)
        {
            this.DynamicScopeValidation(AuthScopes.Helix_Clips_Edit);
            var getParams = new List<KeyValuePair<String, String>>
                {
                    new KeyValuePair<String, String>("broadcaster_id", broadcasterId)
                };
            return this.TwitchPostGenericAsync<CreatedClipResponse>("/clips", ApiVersion.Helix, null, getParams, authToken);
        }

        #endregion

    }
}