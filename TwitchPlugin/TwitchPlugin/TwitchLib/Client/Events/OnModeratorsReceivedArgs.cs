namespace TwitchLib.Client.Events
{
    using System;
    using System.Collections.Generic;

    /// <inheritdoc />
    /// <summary>Args representing a list of moderators received from chat.</summary>
    public class OnModeratorsReceivedArgs : EventArgs
    {
        /// <summary>Property representing the channel the moderator list came from.</summary>
        public String Channel;
        /// <summary>Property representing the list of moderators.</summary>
        public List<String> Moderators;
    }
}
