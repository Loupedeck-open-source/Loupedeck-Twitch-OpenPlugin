namespace TwitchLib.Api.Services.Core.FollowerService
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Helix.Models.Users;
    using Interfaces;

    internal class NameBasedMonitor : CoreMonitor
    {
        private readonly ConcurrentDictionary<String, String> _channelToId = new ConcurrentDictionary<String, String>(StringComparer.OrdinalIgnoreCase);

        public NameBasedMonitor(ITwitchAPI api) : base(api) { }

        public override async Task<GetUsersFollowsResponse> GetUsersFollowsAsync(String channel, Int32 queryCount)
        {
            if (!this._channelToId.TryGetValue(channel, out var channelId))
            {
                channelId = (await this._api.Helix.Users.GetUsersAsync(logins: new List<String> { channel })).Users.FirstOrDefault()?.Id;
                this._channelToId[channel] = channelId ?? throw new InvalidOperationException($"No channel with the name \"{channel}\" could be found.");
            }
            return await this._api.Helix.Users.GetUsersFollowsAsync(first: queryCount, toId: channelId);
        }

        public void ClearCache()
        {
            this._channelToId.Clear();
        }
    }
}