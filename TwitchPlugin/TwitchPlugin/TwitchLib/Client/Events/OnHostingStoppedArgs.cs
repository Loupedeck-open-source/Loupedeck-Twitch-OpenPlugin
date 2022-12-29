namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    /// <inheritdoc />
    /// <summary>Args representing hosting stopped event.</summary>
    public class OnHostingStoppedArgs : EventArgs
    {
        /// <summary>Property representing hosting channel.</summary>
        public HostingStopped HostingStopped;
    }
}
