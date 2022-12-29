namespace TwitchLib.Client.Events
{
    using System;

    /// <inheritdoc />
    /// <summary>Args representing a successful chat color change request.</summary>
    public class OnChatColorChangedArgs : EventArgs
    {
        /// <summary>Property reprenting the channel the event was received in.</summary>
        public String Channel;
    }
}
