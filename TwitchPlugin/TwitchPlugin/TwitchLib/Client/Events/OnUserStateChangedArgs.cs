namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    /// <inheritdoc />
    /// <summary>Args representing on user state changed event.</summary>
    public class OnUserStateChangedArgs : EventArgs
    {
        /// <summary>Property representing user state object.</summary>
        public UserState UserState;
    }
}
