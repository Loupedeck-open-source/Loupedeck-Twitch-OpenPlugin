namespace TwitchLib.Api.Core.RateLimiter
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Interfaces;

    public class ComposedAwaitableConstraint : IAwaitableConstraint
    {
        private IAwaitableConstraint _ac1;
        private IAwaitableConstraint _ac2;
        private readonly SemaphoreSlim _semafore = new SemaphoreSlim(1, 1);

        internal ComposedAwaitableConstraint(IAwaitableConstraint ac1, IAwaitableConstraint ac2)
        {
            this._ac1 = ac1;
            this._ac2 = ac2;
        }

        public async Task<IDisposable> WaitForReadiness(CancellationToken cancellationToken)
        {
            await this._semafore.WaitAsync(cancellationToken);
            IDisposable[] diposables;
            try 
            {
                diposables = await Task.WhenAll(this._ac1.WaitForReadiness(cancellationToken), this._ac2.WaitForReadiness(cancellationToken));
            }
            catch (Exception) 
            {
                this._semafore.Release();
                throw;
            } 
            return new DisposeAction(() => 
            {
                foreach (var diposable in diposables)
                {
                    diposable.Dispose();
                }
                this._semafore.Release();
            });
        }
    }
}
