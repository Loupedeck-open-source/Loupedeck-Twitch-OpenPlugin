namespace TwitchLib.Api.Core.Extensions.System
{
    using global::System.Collections.Generic;

    public static class IEnumerableExtensions
    {
        public static void AddTo<T>(this IEnumerable<T> source, List<T> destination)
        {
            if (source != null) destination.AddRange(source);
        }
    }
}
