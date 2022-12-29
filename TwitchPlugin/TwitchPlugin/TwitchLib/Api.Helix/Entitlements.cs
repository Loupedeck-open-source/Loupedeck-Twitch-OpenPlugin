namespace TwitchLib.Api.Helix
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core;
    using Core.Enums;
    using Core.Exceptions;
    using Core.Interfaces;
    using Models.Entitlements;

    public class Entitlements : ApiBase
    {
        public Entitlements(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings, rateLimiter, http)
        {
        }

        #region CreateEntitlementGrantsUploadURL

        public Task<CreateEntitlementGrantsUploadUrlResponse> CreateEntitlementGrantsUploadUrl(String manifestId, EntitleGrantType type, String url = null, String applicationAccessToken = null)
        {
            if (manifestId == null)
                throw new BadParameterException("manifestId cannot be null");

            var getParams = new List<KeyValuePair<String, String>>
                {
                    new KeyValuePair<String, String>("manifest_id", manifestId)
                };

            switch (type)
            {
                case EntitleGrantType.BulkDropsGrant:
                    getParams.Add(new KeyValuePair<String, String>("type", "bulk_drops_grant"));
                    break;
                default:
                    throw new BadParameterException("Unknown entitlement grant type");
            }

            return this.TwitchGetGenericAsync<CreateEntitlementGrantsUploadUrlResponse>("/entitlements/upload", ApiVersion.Helix, getParams);
        }

        #endregion

    }
}