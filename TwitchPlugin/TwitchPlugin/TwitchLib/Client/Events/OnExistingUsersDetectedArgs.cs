namespace TwitchLib.Client.Events
{
    using System;
    using System.Collections.Generic;

    /// <inheritdoc />
    /// <summary>Args representing existing user(s) detected event.</summary>
    public class OnExistingUsersDetectedArgs : EventArgs
    {
        /// <summary>Property representing string list of existing users.</summary>
        public List<String> Users;
        /// <summary>Property representing channel bot is connected to.</summary>
        public String Channel;
    }
}
