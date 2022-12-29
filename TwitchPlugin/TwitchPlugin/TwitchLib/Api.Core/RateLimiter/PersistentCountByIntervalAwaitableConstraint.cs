namespace TwitchLib.Api.Core.RateLimiter
{
    using System;
    using System.Collections.Generic;
    using Interfaces;

    /// <summary>
    /// <see cref="CountByIntervalAwaitableConstraint"/> that is able to save own state.
    /// </summary>
    public class PersistentCountByIntervalAwaitableConstraint : CountByIntervalAwaitableConstraint
    {
        private readonly Action<DateTime> _saveStateAction;

        /// <summary>
        /// Create an instance of <see cref="PersistentCountByIntervalAwaitableConstraint"/>.
        /// </summary>
        /// <param name="count">Maximum actions allowed per time interval.</param>
        /// <param name="timeSpan">Time interval limits are applied for.</param>
        /// <param name="saveStateAction">Action is used to save state.</param>
        /// <param name="initialTimeStamps">Initial timestamps.</param>
        public PersistentCountByIntervalAwaitableConstraint(Int32 count, TimeSpan timeSpan,
            Action<DateTime> saveStateAction, IEnumerable<DateTime> initialTimeStamps, ITime time = null) : base(count, timeSpan, time)
        {
            this._saveStateAction = saveStateAction;

            if (initialTimeStamps == null)
                return;

            foreach (var timeStamp in initialTimeStamps)
            {
                this._timeStamps.Push(timeStamp);
            }
        }

        /// <summary>
        /// Save state
        /// </summary>
        protected override void OnEnded(DateTime now)
        {
            this._saveStateAction(now);
        }
    }
}