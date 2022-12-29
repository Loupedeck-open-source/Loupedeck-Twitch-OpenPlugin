namespace TwitchLib.Api.Services.Core.LiveStreamMonitor
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Helix.Models.Streams;
    using Interfaces;

    internal class IdBasedMonitor : CoreMonitor
    {
        public IdBasedMonitor(ITwitchAPI api) : base(api) { }

        public override Task<Func<Stream, Boolean>> CompareStream(String channel)
        {
            return Task.FromResult(new Func<Stream, Boolean>(stream => stream.UserId == channel));
        }

        public override Task<GetStreamsResponse> GetStreamsAsync(List<String> channels)
        {
            return this._api.Helix.Streams.GetStreamsAsync(first: channels.Count, userIds: channels);
        }
    }
}
