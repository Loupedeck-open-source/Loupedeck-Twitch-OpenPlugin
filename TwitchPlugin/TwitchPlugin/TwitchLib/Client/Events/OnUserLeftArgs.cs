namespace TwitchLib.Client.Events
{
    using System;

    /// <inheritdoc />
    /// <summary>Args representing viewer left event.</summary>
    public class OnUserLeftArgs : EventArgs
    {
        /// <summary>Property representing username of user that left.</summary>
        public String Username;
        /// <summary>Property representing channel bot is connected to.</summary>
        public String Channel;
    }
}
