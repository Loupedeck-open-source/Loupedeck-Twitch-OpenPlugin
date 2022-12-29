namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    /// <inheritdoc />
    /// <summary>Args representing a user was timed out event.</summary>
    public class OnUserTimedoutArgs : EventArgs
    {
        public UserTimeout UserTimeout;
    }
}
