namespace TwitchLib.Api.Core.Extensions.System
{
    using global::System;
    using global::System.Globalization;

    public static class DateTimeExtensions
    {
        public static String ToRfc3339String(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
        }
    }
}