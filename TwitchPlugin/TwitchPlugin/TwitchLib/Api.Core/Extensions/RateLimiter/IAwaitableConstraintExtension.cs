namespace TwitchLib.Api.Core.Extensions.RateLimiter
{
    using Core.RateLimiter;
    using Interfaces;

    public static class IAwaitableConstraintExtension
    {
        public static IAwaitableConstraint Compose(this IAwaitableConstraint ac1, IAwaitableConstraint ac2)
        {
            return ac1 == ac2 ? ac1 : new ComposedAwaitableConstraint(ac1, ac2);
        }
    }
}
