namespace TwitchLib.Api.Services.Events.LiveStreamMonitor
{
    using System;
    using Helix.Models.Streams;

    /// <inheritdoc />
    /// <summary>
    /// Class representing EventArgs for OnStreamUpdate event.
    /// </summary>
    public class OnStreamUpdateArgs : EventArgs
    {
        /// <summary>
        /// Event property representing channel that has gone online.
        /// </summary>
        public String Channel;
        /// <summary>
        /// Event property representing live stream information.
        /// </summary>
        public Stream Stream;
    }
}
