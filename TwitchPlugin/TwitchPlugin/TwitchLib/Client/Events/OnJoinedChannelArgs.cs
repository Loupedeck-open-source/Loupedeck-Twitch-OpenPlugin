namespace TwitchLib.Client.Events
{
    using System;

    /// <inheritdoc />
    /// <summary>Args representing on channel joined event.</summary>
    public class OnJoinedChannelArgs : EventArgs
    {
        /// <summary>Property representing bot username.</summary>
        public String BotUsername;
        /// <summary>Property representing the channel that was joined.</summary>
        public String Channel;
    }
}
