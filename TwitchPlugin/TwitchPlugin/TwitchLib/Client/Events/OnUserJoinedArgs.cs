namespace TwitchLib.Client.Events
{
    using System;

    /// <inheritdoc />
    /// <summary>Args representing viewer joined event.</summary>
    public class OnUserJoinedArgs : EventArgs
    {
        /// <summary>Property representing username of joined viewer.</summary>
        public String Username;
        /// <summary>Property representing channel bot is connected to.</summary>
        public String Channel;
    }
}
