namespace TwitchLib.Api.Core.Interfaces
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ITime
    {
        DateTime GetTimeNow();

        Task GetDelay(TimeSpan timespan, CancellationToken cancellationToken);
    }
}
