namespace TwitchLib.Api.Services.Core.LiveStreamMonitor
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Helix.Models.Streams;
    using Interfaces;

    internal abstract class CoreMonitor
    {
        protected readonly ITwitchAPI _api;

        public abstract Task<GetStreamsResponse> GetStreamsAsync(List<String> channels);
        public abstract Task<Func<Stream, Boolean>> CompareStream(String channel);

        protected CoreMonitor(ITwitchAPI api)
        {
            this._api = api;
        }
    }
}