namespace TwitchLib.Api.Core.Undocumented
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Enums;
    using Exceptions;
    using Interfaces;
    using Interfaces.Clips;
    using Models.Undocumented.ChannelExtensionData;
    using Models.Undocumented.ChannelPanels;
    using Models.Undocumented.ChatProperties;
    using Models.Undocumented.Chatters;
    using Models.Undocumented.ChatUser;
    using Models.Undocumented.ClipChat;
    using Models.Undocumented.Comments;
    using Models.Undocumented.CSMaps;
    using Models.Undocumented.CSStreams;
    using Models.Undocumented.Hosting;
    using Models.Undocumented.RecentEvents;
    using Models.Undocumented.RecentMessages;
    using Models.Undocumented.TwitchPrimeOffers;

    /// <summary>These endpoints are pretty cool, but they may stop working at anytime due to changes Twitch makes.</summary>
    public class Undocumented : ApiBase
    {
        public Undocumented(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings,
            rateLimiter, http)
        {
        }

        #region GetClipChat

        public async Task<GetClipChatResponse> GetClipChatAsync(IClip clip)
        {
            if (clip == null)
                return null;

            var vodId = $"v{clip.VOD.Id}";
            var offsetTime = clip.VOD.Url.Split('=')[1];
            Int64 offsetSeconds = 2; // for some reason, VODs have 2 seconds behind where clips start

            if (offsetTime.Contains("h"))
            {
                offsetSeconds += Int32.Parse(offsetTime.Split('h')[0]) * 60 * 60;
                offsetTime = offsetTime.Replace(offsetTime.Split('h')[0] + "h", "");
            }

            if (offsetTime.Contains("m"))
            {
                offsetSeconds += Int32.Parse(offsetTime.Split('m')[0]) * 60;
                offsetTime = offsetTime.Replace(offsetTime.Split('m')[0] + "m", "");
            }

            if (offsetTime.Contains("s"))
                offsetSeconds += Int32.Parse(offsetTime.Split('s')[0]);

            var getParams = new List<KeyValuePair<String, String>>
            {
                new KeyValuePair<String, String>("video_id", vodId),
                new KeyValuePair<String, String>("offset_seconds", offsetSeconds.ToString())
            };
            const String rechatResource = "https://rechat.twitch.tv/rechat-messages";
            return await this.GetGenericAsync<GetClipChatResponse>(rechatResource, getParams).ConfigureAwait(false);
        }

        #endregion

        #region GetTwitchPrimeOffers

        public Task<TwitchPrimeOffers> GetTwitchPrimeOffersAsync()
        {
            var getParams = new List<KeyValuePair<String, String>> {new KeyValuePair<String, String>("on_site", "1")};

            return this.GetGenericAsync<TwitchPrimeOffers>("https://api.twitch.tv/api/premium/offers", getParams);
        }

        #endregion

        #region GetChannelHosts

        public Task<ChannelHostsResponse> GetChannelHostsAsync(String channelId)
        {
            var getParams = new List<KeyValuePair<String, String>>
            {
                new KeyValuePair<String, String>("include_logins", "1"),
                new KeyValuePair<String, String>("target", channelId)
            };

            return this.TwitchGetGenericAsync<ChannelHostsResponse>("hosts", ApiVersion.V5, getParams,
                customBase: "https://tmi.twitch.tv/");
        }

        #endregion

        #region GetChatProperties

        public Task<ChatProperties> GetChatPropertiesAsync(String channelName)
        {
            return this.GetGenericAsync<ChatProperties>(
                $"https://api.twitch.tv/api/channels/{channelName}/chat_properties");
        }

        #endregion

        #region GetChannelPanels

        public Task<Panel[]> GetChannelPanelsAsync(String channelName)
        {
            return this.GetGenericAsync<Panel[]>($"https://api.twitch.tv/api/channels/{channelName}/panels");
        }

        #endregion

        #region GetCSMaps

        public Task<CSMapsResponse> GetCSMapsAsync()
        {
            return this.GetGenericAsync<CSMapsResponse>("https://api.twitch.tv/api/cs/maps");
        }

        #endregion

        #region GetCSStreams

        public Task<CSStreams> GetCSStreamsAsync(Int32 limit = 25, Int32 offset = 0)
        {
            var getParams = new List<KeyValuePair<String, String>>
            {
                new KeyValuePair<String, String>("limit", limit.ToString()),
                new KeyValuePair<String, String>("offset", offset.ToString())
            };
            return this.GetGenericAsync<CSStreams>("https://api.twitch.tv/api/cs", getParams);
        }

        #endregion

        #region GetRecentMessages

        public Task<RecentMessagesResponse> GetRecentMessagesAsync(String channelId)
        {
            return this.GetGenericAsync<RecentMessagesResponse>(
                $"https://tmi.twitch.tv/api/rooms/{channelId}/recent_messages");
        }

        #endregion

        #region GetChatters

        public async Task<List<ChatterFormatted>> GetChattersAsync(String channelName)
        {
            var resp = await this.GetGenericAsync<ChattersResponse>(
                $"https://tmi.twitch.tv/group/user/{channelName.ToLower()}/chatters");

            var chatters = resp.Chatters.Staff.Select(chatter => new ChatterFormatted(chatter, UserType.Staff))
                .ToList();
            chatters.AddRange(resp.Chatters.Admins.Select(chatter => new ChatterFormatted(chatter, UserType.Admin)));
            chatters.AddRange(resp.Chatters.GlobalMods.Select(chatter =>
                new ChatterFormatted(chatter, UserType.GlobalModerator)));
            chatters.AddRange(
                resp.Chatters.Moderators.Select(chatter => new ChatterFormatted(chatter, UserType.Moderator)));
            chatters.AddRange(resp.Chatters.Viewers.Select(chatter => new ChatterFormatted(chatter, UserType.Viewer)));

            foreach (var chatter in chatters)
                if (String.Equals(chatter.Username, channelName, StringComparison.CurrentCultureIgnoreCase))
                    chatter.UserType = UserType.Broadcaster;

            return chatters;
        }

        #endregion

        #region GetRecentChannelEvents

        public Task<RecentEvents> GetRecentChannelEventsAsync(String channelId)
        {
            return this.GetGenericAsync<RecentEvents>($"https://api.twitch.tv/bits/channels/{channelId}/events/recent");
        }

        #endregion

        #region GetChatUser

        public Task<ChatUserResponse> GetChatUserAsync(String userId, String channelId = null)
        {
            return this.GetGenericAsync<ChatUserResponse>(channelId != null
                ? $"https://api.twitch.tv/kraken/users/{userId}/chat/channels/{channelId}"
                : $"https://api.twitch.tv/kraken/users/{userId}/chat/");
        }

        #endregion

        #region IsUsernameAvailable

        public Task<Boolean> IsUsernameAvailableAsync(String username)
        {
            var getParams = new List<KeyValuePair<String, String>>
                {new KeyValuePair<String, String>("users_service", "true")};
            var resp = this.RequestReturnResponseCode($"https://passport.twitch.tv/usernames/{username}", "HEAD",
                getParams);
            switch (resp)
            {
                case 200:
                    return Task.FromResult(false);
                case 204:
                    return Task.FromResult(true);
                default:
                    throw new BadResourceException(
                        "Unexpected response from resource. Expecting response code 200 or 204, received: " + resp);
            }
        }

        #endregion

        #region GetChannelExtensionData

        public Task<GetChannelExtensionDataResponse> GetChannelExtensionDataAsync(String channelId)
        {
            return this.TwitchGetGenericAsync<GetChannelExtensionDataResponse>($"/channels/{channelId}/extensions",
                ApiVersion.V5, customBase: "https://api.twitch.tv/v5");
        }

        #endregion

        #region GetComments

        public Task<CommentsPage> GetCommentsPageAsync(String videoId, Int32? contentOffsetSeconds = null,
            String cursor = null)
        {
            var getParams = new List<KeyValuePair<String, String>>();
            if (String.IsNullOrWhiteSpace(videoId))
                throw new BadParameterException(
                    "The video id is not valid. It is not allowed to be null, empty or filled with whitespaces.");

            if (contentOffsetSeconds.HasValue)
                getParams.Add(new KeyValuePair<String, String>("content_offset_seconds",
                    contentOffsetSeconds.Value.ToString()));

            if (cursor != null) getParams.Add(new KeyValuePair<String, String>("cursor", cursor));

            return this.GetGenericAsync<CommentsPage>($"https://api.twitch.tv/kraken/videos/{videoId}/comments",
                getParams);
        }

        public async Task<List<CommentsPage>> GetAllCommentsAsync(String videoId)
        {
            var pages = new List<CommentsPage> {await this.GetCommentsPageAsync(videoId)};
            while (pages.Last().Next != null)
                pages.Add(await this.GetCommentsPageAsync(videoId, null, pages.Last().Next));

            return pages;
        }

        #endregion
    }
}