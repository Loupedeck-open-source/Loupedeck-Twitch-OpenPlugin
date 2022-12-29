﻿namespace TwitchLib.Client.Extensions
{
    using System;
    using Interfaces;
    using Models;

    /// <summary>Extension implementing the ban functionality in TwitchClient.</summary>
    public static class BanUserExt
    {
        /// <summary>
        /// Bans a user in chat using JoinedChannel
        /// </summary>
        /// <param name="channel">JoinedChannel object to send ban to</param>
        /// <param name="viewer">Viewer name to ban</param>
        /// <param name="message">Message to accompany the ban and show the user.</param>
        /// <param name="dryRun">Indicates a dryrun (will not send if true)</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void BanUser(this ITwitchClient client, JoinedChannel channel, String viewer, String message = "", Boolean dryRun = false)
        {
            client.SendMessage(channel, $".ban {viewer} {message}");
        }

        /// <summary>
        /// Bans a user in chat using a string for the channel
        /// </summary>
        /// <param name="channel">Channel in string form to send ban to</param>
        /// <param name="viewer">Viewer name to ban</param>
        /// <param name="message">Message to accompany the ban and show the user.</param>
        /// <param name="dryRun">Indicates a dryrun (will not send if true)</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void BanUser(this ITwitchClient client, String channel, String viewer, String message = "", Boolean dryRun = false)
        {
            var joinedChannel = client.GetJoinedChannel(channel);
            if (joinedChannel != null)
                BanUser(client, joinedChannel, viewer, message, dryRun);
        }
    }
}
