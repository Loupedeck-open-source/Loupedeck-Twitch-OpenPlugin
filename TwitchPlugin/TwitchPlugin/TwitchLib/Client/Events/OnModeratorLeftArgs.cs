namespace TwitchLib.Client.Events
{
    using System;

    /// <inheritdoc />
    /// <summary>Args representing moderator leave event.</summary>
    public class OnModeratorLeftArgs : EventArgs
    {
        /// <summary>Property representing username of moderator that left..</summary>
        public String Username;
        /// <summary>Property representing channel bot is connected to.</summary>
        public String Channel;
    }
}
