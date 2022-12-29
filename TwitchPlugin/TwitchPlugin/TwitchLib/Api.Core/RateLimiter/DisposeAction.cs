namespace TwitchLib.Api.Core.RateLimiter
{
    using System;

    public class DisposeAction : IDisposable
    {
        private Action _act;

        public DisposeAction(Action act) 
        {
            this._act = act;
        }

        public void Dispose() 
        {
            this._act?.Invoke();
            this._act = null;
        }
    }
}
