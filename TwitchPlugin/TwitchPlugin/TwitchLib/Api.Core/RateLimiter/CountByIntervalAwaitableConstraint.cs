namespace TwitchLib.Api.Core.RateLimiter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;

    public class CountByIntervalAwaitableConstraint : IAwaitableConstraint
    {
        public IReadOnlyList<DateTime> TimeStamps => this._timeStamps.ToList();

        protected LimitedSizeStack<DateTime> _timeStamps { get; }

        private Int32 _count { get; }
        private TimeSpan _timeSpan { get; }
        private SemaphoreSlim _semafore { get; } = new SemaphoreSlim(1, 1);
        private ITime _time { get; }

        public CountByIntervalAwaitableConstraint(Int32 count, TimeSpan timeSpan, ITime time=null)
        {
            if (count <= 0)
                throw new ArgumentException("count should be strictly positive", nameof(count));

            if (timeSpan.TotalMilliseconds <= 0)
                throw new ArgumentException("timeSpan should be strictly positive", nameof(timeSpan));

            this._count = count;
            this._timeSpan = timeSpan;
            this._timeStamps = new LimitedSizeStack<DateTime>(this._count);
            this._time = time ?? TimeSystem.StandardTime;
        }

        public async Task<IDisposable> WaitForReadiness(CancellationToken cancellationToken)
        {
            await this._semafore.WaitAsync(cancellationToken);
            var count = 0;
            var now = this._time.GetTimeNow();
            var target = now - this._timeSpan;
            LinkedListNode<DateTime> element = this._timeStamps.First, last = null;
            while ((element != null) && (element.Value > target))
            {
                last = element;
                element = element.Next;
                count++;
            }

            if (count < this._count)
                return new DisposeAction(this.OnEnded);

            var timetoWait = last.Value.Add(this._timeSpan) - now;
            try 
            {
                await this._time.GetDelay(timetoWait, cancellationToken);
            }
            catch (Exception) 
            {
                this._semafore.Release();
                throw;
            }           

            return new DisposeAction(this.OnEnded);
        }

        private void OnEnded() 
        {
            var now = this._time.GetTimeNow();
            this._timeStamps.Push(now);
            this.OnEnded(now);
            this._semafore.Release();
        }

        protected virtual void OnEnded(DateTime now)
        { }
    }
}
