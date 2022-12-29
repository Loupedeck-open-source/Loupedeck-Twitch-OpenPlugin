namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    /// <inheritdoc />
    /// <summary>Args representing chat command received event.</summary>
    public class OnChatCommandReceivedArgs : EventArgs
    {
        /// Property representing received command.
        public ChatCommand Command;
    }
}
