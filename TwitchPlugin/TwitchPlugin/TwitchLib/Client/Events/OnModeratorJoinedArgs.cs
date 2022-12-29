namespace TwitchLib.Client.Events
{
    using System;

    /// <inheritdoc />
    /// <summary>Args representing moderator joined event.</summary>
    public class OnModeratorJoinedArgs : EventArgs
    {
        /// <summary>Property representing username of joined moderator.</summary>
        public String Username;
        /// <summary>Property representing channel bot is connected to.</summary>
        public String Channel;
    }
}
