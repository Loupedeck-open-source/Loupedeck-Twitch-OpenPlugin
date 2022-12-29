namespace TwitchLib.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.LiveStreamMonitor;
    using Events.LiveStreamMonitor;
    using Helix.Models.Streams;
    using Interfaces;

    public class LiveStreamMonitorService : ApiService
    {
        private CoreMonitor _monitor;
        private IdBasedMonitor _idBasedMonitor;
        private NameBasedMonitor _nameBasedMonitor;

        /// <summary>
        /// A cache with streams that are currently live.
        /// </summary>
        public Dictionary<String, Stream> LiveStreams { get; } = new Dictionary<String, Stream>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// The maximum amount of streams to collect per request.
        /// </summary>
        public Int32 MaxStreamRequestCountPerRequest { get; }

        private IdBasedMonitor IdBasedMonitor => this._idBasedMonitor ?? (this._idBasedMonitor = new IdBasedMonitor(this._api));
        private NameBasedMonitor NameBasedMonitor => this._nameBasedMonitor ?? (this._nameBasedMonitor = new NameBasedMonitor(this._api));

        /// <summary>
        /// Invoked when a monitored stream went online.
        /// </summary>
        public event EventHandler<OnStreamOnlineArgs> OnStreamOnline;
        /// <summary>
        /// Invoked when a monitored stream went offline.
        /// </summary>
        public event EventHandler<OnStreamOfflineArgs> OnStreamOffline;
        /// <summary>
        /// Invoked when a monitored stream was already online, but is updated with it's latest information (might be the same).
        /// </summary>
        public event EventHandler<OnStreamUpdateArgs> OnStreamUpdate;

        /// <summary>
        /// The constructor from the LiveStreamMonitorService
        /// </summary>
        /// <exception cref="ArgumentNullException">When the <paramref name="api"/> is null.</exception>
        /// <exception cref="ArgumentException">When the <paramref name="checkIntervalInSeconds"/> is lower than one second.</exception> 
        /// <exception cref="ArgumentException">When the <paramref name="maxStreamRequestCountPerRequest"/> is less than 1 or more than 100.</exception> 
        /// <param name="api">The api used to query information.</param>
        /// <param name="checkIntervalInSeconds"></param>
        /// <param name="maxStreamRequestCountPerRequest"></param>
        public LiveStreamMonitorService(ITwitchAPI api, Int32 checkIntervalInSeconds = 60, Int32 maxStreamRequestCountPerRequest = 100) : 
            base (api, checkIntervalInSeconds)
        {
            if (maxStreamRequestCountPerRequest < 1 || maxStreamRequestCountPerRequest > 100)
                throw new ArgumentException("Twitch doesn't support less than 1 or more than 100 streams per request.", nameof(maxStreamRequestCountPerRequest));

            this.MaxStreamRequestCountPerRequest = maxStreamRequestCountPerRequest;
        }

        public void ClearCache()
        {
            this.LiveStreams.Clear();

            this._nameBasedMonitor?.ClearCache();

            this._nameBasedMonitor = null;
            this._idBasedMonitor = null;
        }

        public void SetChannelsById(List<String> channelsToMonitor)
        {
            this.SetChannels(channelsToMonitor);

            this._monitor = this.IdBasedMonitor;
        }

        public void SetChannelsByName(List<String> channelsToMonitor)
        {
            this.SetChannels(channelsToMonitor);

            this._monitor = this.NameBasedMonitor;
        }

        public async Task UpdateLiveStreamersAsync(Boolean callEvents = true)
        {
            var result = await this.GetLiveStreamersAsync();

            foreach (var channel in this.ChannelsToMonitor)
            {
                var liveStream = result.FirstOrDefault(await this._monitor.CompareStream(channel));

                if (liveStream != null)
                {
                    this.HandleLiveStreamUpdate(channel, liveStream, callEvents);
                }
                else
                {
                    this.HandleOfflineStreamUpdate(channel, callEvents);
                }
            }
        }

        protected override async Task OnServiceTimerTick()
        {
            await base.OnServiceTimerTick();
            await this.UpdateLiveStreamersAsync();
        }

        private void HandleLiveStreamUpdate(String channel, Stream liveStream, Boolean callEvents)
        {
            var wasAlreadyLive = this.LiveStreams.ContainsKey(channel);

            this.LiveStreams[channel] = liveStream;

            if (!callEvents)
                return;

            if (!wasAlreadyLive)
            {
                this.OnStreamOnline?.Invoke(this, new OnStreamOnlineArgs { Channel = channel, Stream = liveStream });
            }
            else
            {
                this.OnStreamUpdate?.Invoke(this, new OnStreamUpdateArgs { Channel = channel, Stream = liveStream });
            }
        }

        private void HandleOfflineStreamUpdate(String channel, Boolean callEvents)
        {
            var wasAlreadyLive = this.LiveStreams.TryGetValue(channel, out var cachedLiveStream);

            if (!wasAlreadyLive)
                return;

            this.LiveStreams.Remove(channel);

            if (!callEvents)
                return;

            this.OnStreamOffline?.Invoke(this, new OnStreamOfflineArgs { Channel = channel, Stream = cachedLiveStream });
        }

        private async Task<List<Stream>> GetLiveStreamersAsync()
        {
            var livestreamers = new List<Stream>();
            var pages = Math.Ceiling((Double)this.ChannelsToMonitor.Count / this.MaxStreamRequestCountPerRequest);

            for (var i = 0; i < pages; i++)
            {
                var selectedSet = this.ChannelsToMonitor.Skip(i * this.MaxStreamRequestCountPerRequest).Take(this.MaxStreamRequestCountPerRequest).ToList();
                var resultset = await this._monitor.GetStreamsAsync(selectedSet);

                if (resultset.Streams == null)
                    continue;

                livestreamers.AddRange(resultset.Streams);
            }
            return livestreamers;
        }
    }
}
