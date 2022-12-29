namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DisplayNameMaps
    {
        public static readonly Dictionary<String, String> SlowModeNamesMap =
            new[] { "1", "3", "5", "10", "15", "30", "60", "120" }.ToDictionary(item => item, item => $"{item} Sec");

        public static readonly Dictionary<String, String> RunCommercialNamesMap =
            new[] { "30", "60", "90", "120", "150", "180" }.ToDictionary(item => item, item => $"{item} Sec");

        public static readonly Dictionary<String, String> FollowersOnlyNamesMap = new Dictionary<String, String>
        {
            {"10m", "10 Minutes"},
            {"30m", "30 Minutes"},
            {"1h", "1 Hour"},
            {"1d", "1 Day"},
            {"1w", "1 Week"},
            {"1mo", "1 Month"},
            {"3mo", "3 Months"},
        };
    }
}