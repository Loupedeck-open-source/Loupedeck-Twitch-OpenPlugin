namespace TwitchLib.Api.Helix
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core;
    using Core.Enums;
    using Core.Exceptions;
    using Core.Interfaces;
    using Models.Videos;

    public class Videos : ApiBase
        {
            public Videos(IApiSettings settings, IRateLimiter rateLimiter, IHttpCallHandler http) : base(settings, rateLimiter, http)
            {
            }

            public Task<GetVideosResponse> GetVideoAsync(List<String> videoIds = null, String userId = null, String gameId = null, String after = null, String before = null, Int32 first = 20, String language = null, Period period = Period.All, VideoSort sort = VideoSort.Time, VideoType type = VideoType.All)
            {
                if ((videoIds == null || videoIds.Count == 0) && userId == null && gameId == null)
                    throw new BadParameterException("VideoIds, userId, and gameId cannot all be null/empty.");
                if (videoIds != null && videoIds.Count > 0 && userId != null || videoIds != null && videoIds.Count > 0 && gameId != null || userId != null && gameId != null)
                    throw new BadParameterException("If videoIds are present, you may not use userid or gameid. If gameid is present, you may not use videoIds or userid. If userid is present, you may not use videoids or gameids.");

                var getParams = new List<KeyValuePair<String, String>>();
                if (videoIds != null && videoIds.Count > 0)
                {
                    foreach (var videoId in videoIds)
                        getParams.Add(new KeyValuePair<String, String>("id", videoId));
                }

                if (userId != null)
                    getParams.Add(new KeyValuePair<String, String>("user_id", userId));
                if (gameId != null)
                    getParams.Add(new KeyValuePair<String, String>("game_id", gameId));

                if (userId != null || gameId != null)
                {
                    if (after != null)
                        getParams.Add(new KeyValuePair<String, String>("after", after));
                    if (before != null)
                        getParams.Add(new KeyValuePair<String, String>("before", before));
                    getParams.Add(new KeyValuePair<String, String>("first", first.ToString()));
                    if (language != null)
                        getParams.Add(new KeyValuePair<String, String>("language", language));
                    switch (period)
                    {
                        case Period.All:
                            getParams.Add(new KeyValuePair<String, String>("period", "all"));
                            break;
                        case Period.Day:
                            getParams.Add(new KeyValuePair<String, String>("period", "day"));
                            break;
                        case Period.Month:
                            getParams.Add(new KeyValuePair<String, String>("period", "month"));
                            break;
                        case Period.Week:
                            getParams.Add(new KeyValuePair<String, String>("period", "week"));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(period), period, null);
                    }

                    switch (sort)
                    {
                        case VideoSort.Time:
                            getParams.Add(new KeyValuePair<String, String>("sort", "time"));
                            break;
                        case VideoSort.Trending:
                            getParams.Add(new KeyValuePair<String, String>("sort", "trending"));
                            break;
                        case VideoSort.Views:
                            getParams.Add(new KeyValuePair<String, String>("sort", "views"));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(sort), sort, null);
                    }

                    switch (type)
                    {
                        case VideoType.All:
                            getParams.Add(new KeyValuePair<String, String>("type", "all"));
                            break;
                        case VideoType.Highlight:
                            getParams.Add(new KeyValuePair<String, String>("type", "highlight"));
                            break;
                        case VideoType.Archive:
                            getParams.Add(new KeyValuePair<String, String>("type", "archive"));
                            break;
                        case VideoType.Upload:
                            getParams.Add(new KeyValuePair<String, String>("type", "upload"));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                }

                return this.TwitchGetGenericAsync<GetVideosResponse>("/videos", ApiVersion.Helix, getParams);
            }
        }
    
}