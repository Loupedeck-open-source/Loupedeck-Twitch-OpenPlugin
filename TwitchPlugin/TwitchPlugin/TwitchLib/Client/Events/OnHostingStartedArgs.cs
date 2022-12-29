namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    /// <inheritdoc />
    /// <summary>Args representing hosting started event.</summary>
    public class OnHostingStartedArgs : EventArgs
    {
        /// <summary>Property representing hosting channel.</summary>
        public HostingStarted HostingStarted;
    }
}
