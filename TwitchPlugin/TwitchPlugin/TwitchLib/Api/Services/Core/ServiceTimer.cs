namespace TwitchLib.Api.Services.Core
{
    using System;
    using System.Threading.Tasks;
    using System.Timers;

    internal class ServiceTimer : Timer
    {
        public Int32 IntervalInSeconds { get; }

        public delegate Task ServiceTimerTick();

        private readonly ServiceTimerTick _serviceTimerTickAsyncCallback;

        public ServiceTimer(ServiceTimerTick serviceTimerTickAsyncCallback, Int32 intervalInSeconds = 60)
        {
            this._serviceTimerTickAsyncCallback = serviceTimerTickAsyncCallback;
            this.Interval = intervalInSeconds * 1000;
            this.IntervalInSeconds = intervalInSeconds;
            this.Elapsed += this.TimerElapsedAsync;
        }

        private async void TimerElapsedAsync(Object sender, ElapsedEventArgs e)
        {
            await this._serviceTimerTickAsyncCallback();
        }
    }
}
