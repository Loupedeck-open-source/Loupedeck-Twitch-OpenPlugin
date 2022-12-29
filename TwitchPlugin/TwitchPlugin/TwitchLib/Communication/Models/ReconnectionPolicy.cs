namespace TwitchLib.Communication.Models
{
    using System;

    public class ReconnectionPolicy
    {
        private readonly Int32 _reconnectStepInterval;
        private readonly Int32? _initMaxAttempts;
        private Int32 _minReconnectInterval;
        private readonly Int32 _maxReconnectInterval;
        private Int32? _maxAttempts;
        private Int32 _attemptsMade;

        public ReconnectionPolicy()
        {
            this._reconnectStepInterval = 3000;
            this._minReconnectInterval = 3000;
            this._maxReconnectInterval = 30000;
            this._maxAttempts = null;
            this._initMaxAttempts = null;
            this._attemptsMade = 0;
        }

        public void SetMaxAttempts(Int32 attempts)
        {
            this._maxAttempts = attempts;
        }

        public void Reset()
        {
            this._attemptsMade = 0;
            this._minReconnectInterval = this._reconnectStepInterval;
            this._maxAttempts = this._initMaxAttempts;
        }

        public void SetAttemptsMade(Int32 count) => this._attemptsMade = count;

        public ReconnectionPolicy(Int32 minReconnectInterval, Int32 maxReconnectInterval, Int32? maxAttempts)
        {
            this._reconnectStepInterval = minReconnectInterval;
            this._minReconnectInterval = minReconnectInterval > maxReconnectInterval
                ? maxReconnectInterval
                : minReconnectInterval;
            this._maxReconnectInterval = maxReconnectInterval;
            this._maxAttempts = maxAttempts;
            this._initMaxAttempts = maxAttempts;
            this._attemptsMade = 0;
        }

        public ReconnectionPolicy(Int32 minReconnectInterval, Int32 maxReconnectInterval)
        {
            this._reconnectStepInterval = minReconnectInterval;
            this._minReconnectInterval = minReconnectInterval > maxReconnectInterval
                ? maxReconnectInterval
                : minReconnectInterval;
            this._maxReconnectInterval = maxReconnectInterval;
            this._maxAttempts = null;
            this._initMaxAttempts = null;
            this._attemptsMade = 0;
        }

        public ReconnectionPolicy(Int32 reconnectInterval)
        {
            this._reconnectStepInterval = reconnectInterval;
            this._minReconnectInterval = reconnectInterval;
            this._maxReconnectInterval = reconnectInterval;
            this._maxAttempts = null;
            this._initMaxAttempts = null;
            this._attemptsMade = 0;
        }

        public ReconnectionPolicy(Int32 reconnectInterval, Int32? maxAttempts)
        {
            this._reconnectStepInterval = reconnectInterval;
            this._minReconnectInterval = reconnectInterval;
            this._maxReconnectInterval = reconnectInterval;
            this._maxAttempts = maxAttempts;
            this._initMaxAttempts = maxAttempts;
            this._attemptsMade = 0;
        }

        internal void ProcessValues()
        {
            this._attemptsMade++;
            if (this._minReconnectInterval < this._maxReconnectInterval)
                this._minReconnectInterval += this._reconnectStepInterval;
            if (this._minReconnectInterval > this._maxReconnectInterval)
                this._minReconnectInterval = this._maxReconnectInterval;
        }

        public Int32 GetReconnectInterval() => this._minReconnectInterval;

        public Boolean AreAttemptsComplete() => this._attemptsMade == this._maxAttempts;
    }
}