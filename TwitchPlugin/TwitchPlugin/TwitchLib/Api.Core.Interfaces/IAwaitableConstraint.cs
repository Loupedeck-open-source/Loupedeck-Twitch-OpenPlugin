namespace TwitchLib.Api.Core.Interfaces
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAwaitableConstraint
    {
        Task<IDisposable> WaitForReadiness(CancellationToken cancellationToken);
    }
}
