namespace TwitchLib.Api.Core.RateLimiter
{
    using System;
    using System.Collections.Generic;

    public class LimitedSizeStack<T>: LinkedList<T>
    {
        private readonly Int32 _maxSize;
        public LimitedSizeStack(Int32 maxSize)
        {
            this._maxSize = maxSize;
        }

        public void Push(T item)
        {
            this.AddFirst(item);

            if (this.Count > this._maxSize)
                this.RemoveLast();
        }
    }
}
