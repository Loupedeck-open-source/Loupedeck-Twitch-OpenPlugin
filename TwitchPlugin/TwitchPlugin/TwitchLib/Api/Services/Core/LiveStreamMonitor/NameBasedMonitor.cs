namespace TwitchLib.Api.Services.Core.LiveStreamMonitor
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Helix.Models.Streams;
    using Interfaces;

    internal class NameBasedMonitor : CoreMonitor
    {
        private readonly ConcurrentDictionary<String, String> _channelToId = new ConcurrentDictionary<String, String>(StringComparer.OrdinalIgnoreCase);

        public NameBasedMonitor(ITwitchAPI api) : base(api) { }

        public override async Task<Func<Stream, Boolean>> CompareStream(String channel)
        {
            if (!this._channelToId.TryGetValue(channel, out var channelId))
            {
                channelId = (await this._api.Helix.Users.GetUsersAsync(logins: new List<String> { channel })).Users.FirstOrDefault()?.Id;
                this._channelToId[channel] = channelId ?? throw new InvalidOperationException($"No channel with the name \"{channel}\" could be found.");
            }

            return stream => stream.UserId == channelId;
        }

        public override Task<GetStreamsResponse> GetStreamsAsync(List<String> channels)
        {
            return this._api.Helix.Streams.GetStreamsAsync(first: channels.Count, userLogins: channels);
        }

        public void ClearCache()
        {
            this._channelToId.Clear();
        }
    }
}