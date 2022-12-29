namespace TwitchLib.Api.Services.Core.FollowerService
{
    using System;
    using System.Threading.Tasks;
    using Helix.Models.Users;
    using Interfaces;

    internal abstract class CoreMonitor
    {
        protected readonly ITwitchAPI _api;

        public abstract Task<GetUsersFollowsResponse> GetUsersFollowsAsync(String channel, Int32 queryCount);

        protected CoreMonitor(ITwitchAPI api)
        {
            this._api = api;
        }
    }
}