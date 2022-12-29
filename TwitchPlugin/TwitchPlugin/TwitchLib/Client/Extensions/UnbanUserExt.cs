﻿namespace TwitchLib.Client.Extensions
{
    using System;
    using Interfaces;
    using Models;

    /// <summary>Extension to implement unban functionality.</summary>
    public static class UnbanUserExt
    {
        /// <summary>
        /// Unbans a user in chat using JoinedChannel
        /// </summary>
        /// <param name="channel">JoinedChannel object to send unban to</param>
        /// <param name="viewer">Viewer name to unban</param>
        /// <param name="dryRun">Indicates a dryrun (will not send if true)</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void UnbanUser(this ITwitchClient client, JoinedChannel channel, String viewer, Boolean dryRun = false)
        {
            client.SendMessage(channel, $".unban {viewer}", dryRun);
        }

        /// <summary>
        /// Unbans a user in chat using a string for the channel
        /// </summary>
        /// <param name="channel">Channel in string form to send unban to</param>
        /// <param name="viewer">Viewer name to unban</param>
        /// <param name="dryRun">Indicates a dryrun (will not send if true)</param>
        /// /// <param name="client">Client reference used to identify extension.</param>
        public static void UnbanUser(this ITwitchClient client, String channel, String viewer, Boolean dryRun = false)
        {
            var joinedChannel = client.GetJoinedChannel(channel);
            if (joinedChannel != null)
                UnbanUser(client, joinedChannel, viewer, dryRun);
        }
    }
}
