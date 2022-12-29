namespace TwitchLib.Client.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception thrown when an event is subscribed to when it shouldn't be.</summary>
    public class BadListenException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public BadListenException(String eventName, String additionalDetails = "")
            : base(String.IsNullOrEmpty(additionalDetails)
                ? $"You are listening to event '{eventName}', which is not currently allowed. See details: {additionalDetails}"
                : $"You are listening to event '{eventName}', which is not currently allowed.")
        {
        }
    }
}
