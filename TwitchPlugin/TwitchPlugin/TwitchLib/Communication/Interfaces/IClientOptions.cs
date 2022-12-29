namespace TwitchLib.Communication.Interfaces
{
    using System;
    using Enums;
    using Models;

    public interface IClientOptions
    {
        /// <summary>
        /// Type of the Client to Create. Possible Types Chat or PubSub.
        /// </summary>
        ClientType ClientType { get; set; }

        /// <summary>
        /// How long to wait on a clean disconnect [in ms] (default 20000ms).
        /// </summary>
        Int32 DisconnectWait { get; set; }

        /// <summary>
        /// Number of Messages Allowed Per Instance of the Throttling Period. (default 100)
        /// </summary>
        Int32 MessagesAllowedInPeriod { get; set; }

        /// <summary>
        /// Reconnection Policy Settings. Reconnect without Losing data etc.
        /// The Default Policy applied is 10 reconnection attempts with 3 seconds between each attempt.
        /// </summary>
        ReconnectionPolicy ReconnectionPolicy { get; set; }

        /// <summary>
        /// The amount of time an object can wait to be sent before it is considered dead, and should be skipped (default 30 minutes).
        /// A dead item will be ignored and removed from the send queue when it is hit.
        /// </summary>
        TimeSpan SendCacheItemTimeout { get; set; }

        /// <summary>
        /// Minimum time between sending items from the queue [in ms] (default 50ms).
        /// </summary>
        UInt16 SendDelay { get; set; }

        /// <summary>
        /// Maximum number of Queued outgoing messages (default 10000).
        /// </summary>
        Int32 SendQueueCapacity { get; set; }

        /// <summary>
        /// Period Between each reset of the throttling instance window. (default 30s)
        /// </summary>
        TimeSpan ThrottlingPeriod { get; set; }

        /// <summary>
        /// Use Secure Connection [SSL] (default: true)
        /// </summary>
        Boolean UseSsl { get; set; }

        /// <summary>
        /// Period Between each reset of the whisper throttling instance window. (default 60s)
        /// </summary>
        TimeSpan WhisperThrottlingPeriod { get; set; }

        /// <summary>
        /// Number of Whispers Allowed to be sent Per Instance of the Throttling Period. (default 100)
        /// </summary>
        Int32 WhispersAllowedInPeriod { get; set; }

        /// <summary>
        /// Maximum number of Queued outgoing Whispers (default 10000).
        /// </summary>
        Int32 WhisperQueueCapacity { get; set; }
    }
}