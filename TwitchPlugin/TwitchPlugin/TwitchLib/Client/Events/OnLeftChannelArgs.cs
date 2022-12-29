namespace TwitchLib.Client.Events
{
    using System;

    /// <inheritdoc />
    /// <summary>Args representing the client left a channel event.</summary>
    public class OnLeftChannelArgs : EventArgs
    {
        /// <summary>The username of the bot that left the channel.</summary>
        public String BotUsername;
        /// <summary>Channel that bot just left (parted).</summary>
        public String Channel;
    }
}
