namespace TwitchLib.Api.Services.Events.LiveStreamMonitor
{
    using System;
    using Helix.Models.Streams;

    /// <inheritdoc />
    /// <summary>
    /// Class representing EventArgs for OnStreamOffline event.
    /// </summary>
    public class OnStreamOfflineArgs : EventArgs
    {
        /// <summary>
        /// The channel that has gone online.
        /// </summary>
        public String Channel;
        /// <summary>
        /// The channel's live stream information.
        /// </summary>
        public Stream Stream;
    }
}
