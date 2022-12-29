namespace TwitchLib.Client.Events
{
    using System;

    /// <inheritdoc />
    /// <summary>Args representing client disconnect event.</summary>
    public class OnDisconnectedArgs : EventArgs
    {
        /// <summary>Username of the bot that was disconnected.</summary>
        public String BotUsername;
    }
}
