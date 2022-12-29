namespace TwitchLib.Api.Services.Events.FollowerService
{
    using System;
    using System.Collections.Generic;
    using Helix.Models.Users;

    /// <inheritdoc />
    /// <summary>
    /// Class representing EventArgs for OnNewFollowersDetected event.
    /// </summary>
    public class OnNewFollowersDetectedArgs : EventArgs
    {
        /// <summary>Event property representing channel the service is currently monitoring.</summary>
        public String Channel;
        /// <summary>Event property representing all new followers detected.</summary>
        public List<Follow> NewFollowers;
    }
}
