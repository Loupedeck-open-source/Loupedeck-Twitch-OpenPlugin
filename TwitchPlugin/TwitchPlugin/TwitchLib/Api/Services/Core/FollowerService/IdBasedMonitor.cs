namespace TwitchLib.Api.Services.Core.FollowerService
{
    using System;
    using System.Threading.Tasks;
    using Helix.Models.Users;
    using Interfaces;

    internal class IdBasedMonitor : CoreMonitor
    {
        public IdBasedMonitor(ITwitchAPI api) : base(api) { }

        public override Task<GetUsersFollowsResponse> GetUsersFollowsAsync(String channel, Int32 queryCount)
        {
            return this._api.Helix.Users.GetUsersFollowsAsync(first: queryCount, toId: channel);
        }
    }
}
