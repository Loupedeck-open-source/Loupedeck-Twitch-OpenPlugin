﻿namespace TwitchLib.Client.Extensions
{
    using System;
    using Interfaces;
    using Models;

    /// <summary>Extension for implementing emote only mode functionality in TwitchClient</summary>
    public static class EmoteOnlyExt
    {
        /// <summary>
        /// Enables emote only chat requirement.
        /// </summary>
        /// <param name="channel">JoinedChannel representation of the channel to send the enable emote only command to.</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void EmoteOnlyOn(this ITwitchClient client, JoinedChannel channel)
        {
            client.SendMessage(channel, ".emoteonly");
        }

        /// <summary>
        /// Enables emote only chat requirement.
        /// </summary>
        /// <param name="channel">String representation of the channel to send the enable emote only command to.</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void EmoteOnlyOn(this ITwitchClient client, String channel)
        {
            client.SendMessage(channel, ".emoteonly");
        }

        /// <summary>
        /// Disables emote only chat requirement.
        /// </summary>
        /// <param name="channel">JoinedChannel representation of the channel to send the disable emote only command to.</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void EmoteOnlyOff(this ITwitchClient client, JoinedChannel channel)
        {
            client.SendMessage(channel, ".emoteonlyoff");
        }

        /// <summary>
        /// Disables emote only chat requirement.
        /// </summary>
        /// <param name="channel">String representation of the channel to send the disable emote only command to.</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void EmoteOnlyOff(this ITwitchClient client, String channel)
        {
            client.SendMessage(channel, ".emoteonlyoff");
        }
    }
}
