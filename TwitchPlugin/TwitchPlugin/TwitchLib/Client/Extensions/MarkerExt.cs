﻿namespace TwitchLib.Client.Extensions
{
    using System;
    using Interfaces;
    using Models;

    /// <summary>Extension for implementing marker functionality</summary>
    public static class MarkerExt
    {
        /// <summary>
        /// Sends command to create a marker using a JoinedChannel object.
        /// </summary>
        /// <param name="channel">JoinedChannel representation of the channel to send the marker command to.</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void Marker(this ITwitchClient client, JoinedChannel channel)
        {
            client.SendMessage(channel, "/marker");
        }

        /// <summary>
        /// Sends command to create a marker using a string.
        /// </summary>
        /// <param name="channel">String representation of the channel to send the marker command to.</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void Marker(this ITwitchClient client, String channel)
        {
            client.SendMessage(channel, "/marker");
        }
    }
}
