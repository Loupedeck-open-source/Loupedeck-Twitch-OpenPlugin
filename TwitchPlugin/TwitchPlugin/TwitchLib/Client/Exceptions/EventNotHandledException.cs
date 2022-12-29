namespace TwitchLib.Client.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception thrown when an event that is not handled is required to be handled.</summary>
    public class EventNotHandled : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public EventNotHandled(String eventName, String additionalDetails = "")
            : base(String.IsNullOrEmpty(additionalDetails) 
                  ? $"To use this call, you must handle/subscribe to event: {eventName}" 
                  : $"To use this call, you must handle/subscribe to event: {eventName}, additional details: {additionalDetails}")
        {
        }
    }
}
