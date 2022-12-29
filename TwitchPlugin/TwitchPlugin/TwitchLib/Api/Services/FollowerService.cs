namespace TwitchLib.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.FollowerService;
    using Events.FollowerService;
    using Helix.Models.Users;
    using Interfaces;

    public class FollowerService : ApiService
    {
        private readonly Dictionary<String, DateTime> _lastFollowerDates = new Dictionary<String, DateTime>(StringComparer.OrdinalIgnoreCase);

        private CoreMonitor _monitor;
        private IdBasedMonitor _idBasedMonitor;
        private NameBasedMonitor _nameBasedMonitor;

        /// <summary>
        /// The current known followers for each channel.
        /// </summary>
        public Dictionary<String, List<Follow>> KnownFollowers { get; } = new Dictionary<String, List<Follow>>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// The amount of followers queried per request.
        /// </summary>
        public Int32 QueryCountPerRequest { get; }
        /// <summary>
        /// The maximum amount of followers cached per channel.
        /// </summary>
        public Int32 CacheSize { get; }

        private IdBasedMonitor IdBasedMonitor => this._idBasedMonitor ?? (this._idBasedMonitor = new IdBasedMonitor(this._api));
        private NameBasedMonitor NameBasedMonitor => this._nameBasedMonitor ?? (this._nameBasedMonitor = new NameBasedMonitor(this._api));

        /// <summary>
        /// Event which is called when new followers are detected.
        /// </summary>
        public event EventHandler<OnNewFollowersDetectedArgs> OnNewFollowersDetected;
        
        /// <summary>
        /// FollowerService constructor.
        /// </summary>
        /// <exception cref="ArgumentNullException">When the <paramref name="api"/> is null.</exception>
        /// <exception cref="ArgumentException">When the <paramref name="checkIntervalInSeconds"/> is lower than one second.</exception> 
        /// <exception cref="ArgumentException">When the <paramref name="queryCountPerRequest" /> is less than 1 or more than 100 followers per request.</exception>
        /// <exception cref="ArgumentException">When the <paramref name="cacheSize" /> is less than the queryCountPerRequest.</exception>
        /// <param name="api">The api to use for querying followers.</param>
        /// <param name="checkIntervalInSeconds">How often new followers should be queried.</param>
        /// <param name="queryCountPerRequest">The amount of followers to query per request.</param>
        /// <param name="cacheSize">The maximum amount of followers to cache per channel.</param>
        public FollowerService(ITwitchAPI api, Int32 checkIntervalInSeconds = 60, Int32 queryCountPerRequest = 100, Int32 cacheSize = 1000) : 
            base(api, checkIntervalInSeconds)
        {
            if (queryCountPerRequest < 1 || queryCountPerRequest > 100)
                throw new ArgumentException("Twitch doesn't support less than 1 or more than 100 followers per request.", nameof(queryCountPerRequest));

            if (cacheSize < queryCountPerRequest)
                throw new ArgumentException($"The cache size must be at least the size of the {nameof(queryCountPerRequest)} parameter.", nameof(cacheSize));

            this.QueryCountPerRequest = queryCountPerRequest;
            this.CacheSize = cacheSize;
        }

        /// <summary>
        /// Clears the existing cache.
        /// </summary>
        public void ClearCache()
        {
            this.KnownFollowers.Clear();

            this._lastFollowerDates.Clear();

            this._nameBasedMonitor?.ClearCache();

            this._nameBasedMonitor = null;
            this._idBasedMonitor = null;
        }

        /// <summary>
        /// Sets the channels to monitor by id. Event's channel properties will be Ids in this case.
        /// </summary>
        /// <exception cref="ArgumentNullException">When <paramref name="channelsToMonitor"/> is null.</exception>
        /// <exception cref="ArgumentException">When <paramref name="channelsToMonitor"/> is empty.</exception>
        /// <param name="channelsToMonitor">A list with channels to monitor.</param>
        public void SetChannelsById(List<String> channelsToMonitor)
        {
            this.SetChannels(channelsToMonitor);

            this._monitor = this.IdBasedMonitor;
        }

        /// <summary>
        /// Sets the channels to monitor by name. Event's channel properties will be names in this case.
        /// </summary>
        /// <exception cref="ArgumentNullException">When <paramref name="channelsToMonitor"/> is null.</exception>
        /// <exception cref="ArgumentException">When <paramref name="channelsToMonitor"/> is empty.</exception>
        /// <param name="channelsToMonitor">A list with channels to monitor.</param>
        public void SetChannelsByName(List<String> channelsToMonitor)
        {
            this.SetChannels(channelsToMonitor);

            this._monitor = this.NameBasedMonitor;
        }

        /// <summary>
        /// Updates the followerservice with the latest followers. Automatically called internally when service is started.
        /// </summary>
        /// <param name="callEvents">Whether to invoke the update events or not.</param>
        public async Task UpdateLatestFollowersAsync(Boolean callEvents = true)
        {
            if (this.ChannelsToMonitor == null)
                return;

            foreach (var channel in this.ChannelsToMonitor)
            {
                List<Follow> newFollowers;
                var latestFollowers = await this.GetLatestFollowersAsync(channel);

                if (latestFollowers.Count == 0)
                    return;

                if (!this.KnownFollowers.TryGetValue(channel, out var knownFollowers))
                {
                    newFollowers = latestFollowers;
                    this.KnownFollowers[channel] = latestFollowers.Take(this.CacheSize).ToList();
                    this._lastFollowerDates[channel] = latestFollowers.Last().FollowedAt;
                }
                else
                {
                    var existingFollowerIds = new HashSet<String>(knownFollowers.Select(f => f.FromUserId));
                    var latestKnownFollowerDate = this._lastFollowerDates[channel];
                    newFollowers = new List<Follow>();

                    foreach (var follower in latestFollowers)
                    {
                        if (!existingFollowerIds.Add(follower.FromUserId)) continue;

                        if (follower.FollowedAt < latestKnownFollowerDate) continue;

                        newFollowers.Add(follower);
                        latestKnownFollowerDate = follower.FollowedAt;
                        knownFollowers.Add(follower);
                    }

                    existingFollowerIds.Clear();
                    existingFollowerIds.TrimExcess();

                    // prune cache so we don't use too much space unnecessarily
                    if (knownFollowers.Count > this.CacheSize)
                        knownFollowers.RemoveRange(0, knownFollowers.Count - this.CacheSize);

                    if (newFollowers.Count <= 0)
                        return;

                    this._lastFollowerDates[channel] = latestKnownFollowerDate;
                }

                if (!callEvents)
                    return;

                this.OnNewFollowersDetected?.Invoke(this, new OnNewFollowersDetectedArgs { Channel = channel, NewFollowers = newFollowers });
            }
        }

        protected override async Task OnServiceTimerTick()
        {
            await base.OnServiceTimerTick();
            await this.UpdateLatestFollowersAsync();
        }

        private async Task<List<Follow>> GetLatestFollowersAsync(String channel)
        {
            var resultset = await this._monitor.GetUsersFollowsAsync(channel, this.QueryCountPerRequest);
            
            return resultset.Follows.Reverse().ToList();
        }
    }
}
