namespace TwitchLib.Api.Helix
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core;
    using Core.Enums;
    using Core.Interfaces;
    using Models.Users;
    using Models.Users.Internal;
    using Newtonsoft.Json.Linq;

    public class Users : ApiBase
    {
        public Users(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings, rateLimiter, http)
        {
        }

        public Task<GetUsersResponse> GetUsersAsync(List<String> ids = null, List<String> logins = null, String accessToken = null)
        {
            var getParams = new List<KeyValuePair<String, String>>();
            if (ids != null && ids.Count > 0)
            {
                foreach (var id in ids)
                    getParams.Add(new KeyValuePair<String, String>("id", id));
            }

            if (logins != null && logins.Count > 0)
            {
                foreach (var login in logins)
                    getParams.Add(new KeyValuePair<String, String>("login", login));
            }

            return this.TwitchGetGenericAsync<GetUsersResponse>("/users", ApiVersion.Helix, getParams, accessToken);
        }

        public Task<GetUsersFollowsResponse> GetUsersFollowsAsync(String after = null, String before = null, Int32 first = 20, String fromId = null, String toId = null)
        {
            var getParams = new List<KeyValuePair<String, String>>
                {
                    new KeyValuePair<String, String>("first", first.ToString())
                };
            if (after != null)
                getParams.Add(new KeyValuePair<String, String>("after", after));
            if (before != null)
                getParams.Add(new KeyValuePair<String, String>("before", before));
            if (fromId != null)
                getParams.Add(new KeyValuePair<String, String>("from_id", fromId));
            if (toId != null)
                getParams.Add(new KeyValuePair<String, String>("to_id", toId));

            return this.TwitchGetGenericAsync<GetUsersFollowsResponse>("/users/follows", ApiVersion.Helix, getParams);
        }

        public Task PutUsersAsync(String description, String accessToken = null)
        {
            var getParams = new List<KeyValuePair<String, String>>
                {
                    new KeyValuePair<String, String>("description", description)
                };

            return this.TwitchPutAsync("/users", ApiVersion.Helix, null, getParams, accessToken);
        }

        public Task<GetUserExtensionsResponse> GetUserExtensionsAsync(String authToken = null)
        {
            return this.TwitchGetGenericAsync<GetUserExtensionsResponse>("/users/extensions/list", ApiVersion.Helix, accessToken: authToken);
        }

        public Task<GetUserActiveExtensionsResponse> GetUserActiveExtensionsAsync(String authToken = null)
        {
            return this.TwitchGetGenericAsync<GetUserActiveExtensionsResponse>("/users/extensions", ApiVersion.Helix, accessToken: authToken);
        }

        public Task<GetUserActiveExtensionsResponse> UpdateUserExtensionsAsync(IEnumerable<ExtensionSlot> userExtensionStates, String authToken = null)
        {
            this.DynamicScopeValidation(AuthScopes.Channel_Editor, authToken);

            var panels = new Dictionary<String, UserExtensionState>();
            var overlays = new Dictionary<String, UserExtensionState>();
            var components = new Dictionary<String, UserExtensionState>();

            foreach (var extension in userExtensionStates)
                switch (extension.Type)
                {
                    case ExtensionType.Component:
                        components.Add(extension.Slot, extension.UserExtensionState);
                        break;
                    case ExtensionType.Overlay:
                        overlays.Add(extension.Slot, extension.UserExtensionState);
                        break;
                    case ExtensionType.Panel:
                        panels.Add(extension.Slot, extension.UserExtensionState);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ExtensionType));
                }

            var json = new JObject();
            var p = new UpdateUserExtensionsRequest();

            if (panels.Count > 0)
                p.Panel = panels;

            if (overlays.Count > 0)
                p.Overlay = overlays;

            if (components.Count > 0)
                p.Component = components;

            json.Add(new JProperty("data", JObject.FromObject(p)));
            var payload = json.ToString();

            return this.TwitchPutGenericAsync<GetUserActiveExtensionsResponse>("/users/extensions", ApiVersion.Helix, payload, accessToken: authToken);
        }
    }
}
