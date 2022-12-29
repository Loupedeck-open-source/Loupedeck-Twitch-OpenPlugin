namespace TwitchLib.Client.Events
{
    using System;

    /// <inheritdoc />
    /// <summary>Args representing on connected event.</summary>
    public class OnConnectedArgs : EventArgs
    {
        /// <summary>Property representing bot username.</summary>
        public String BotUsername;
        /// <summary>Property representing connected channel.</summary>
        public String AutoJoinChannel;
    }
}
