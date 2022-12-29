namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    /// <inheritdoc />
    /// <summary>Args representing a user was banned event.</summary>
    public class OnUserBannedArgs : EventArgs
    {
        public UserBan UserBan;
    }
}
