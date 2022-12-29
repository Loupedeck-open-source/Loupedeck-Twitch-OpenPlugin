﻿namespace TwitchLib.Client.Extensions
{
    using System;
    using Exceptions;
    using Interfaces;
    using Models;

    /// <summary>Extension to implement slowmode functionality in TwitchClient</summary>
    public static class SlowModeExt
    {
        /// <summary>
        /// Enables slow mode. messageCooldown must be less than 1 day.
        /// </summary>
        /// <param name="messageCooldown">TimeSpan object representing how long message cooldowns should be. May not exceed 1 day total.</param>
        /// <param name="channel">JoinedChannel representation of which channel to send the slow command to.</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void SlowModeOn(this ITwitchClient client, JoinedChannel channel, TimeSpan messageCooldown)
        {
            if (messageCooldown > TimeSpan.FromDays(1))
                throw new InvalidParameterException("The message cooldown time supplied exceeded the maximum allowed by Twitch, which is 1 day.", client.TwitchUsername);

            client.SendMessage(channel, $".slow {messageCooldown.TotalSeconds}");
        }

        /// <summary>
        /// Enables slow mode. messageCooldown must be less than 1 day.
        /// </summary>
        /// <param name="messageCooldown">TimeSpan object representing how long message cooldowns should be. May not exceed 1 day total.</param>
        /// <param name="channel">String representation of which channel to send the slow command to.</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void SlowModeOn(this ITwitchClient client, String channel, TimeSpan messageCooldown)
        {
            if (messageCooldown > TimeSpan.FromDays(1))
                throw new InvalidParameterException("The message cooldown time supplied exceeded the maximum allowed by Twitch, which is 1 day.", client.TwitchUsername);

            client.SendMessage(channel, $".slow {messageCooldown.TotalSeconds}");
        }

        /// <summary>
        /// Disables slow mode.
        /// </summary>
        /// <param name="channel">JoinedChannel representation of which channel to send slowoff command to.</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void SlowModeOff(this ITwitchClient client, JoinedChannel channel)
        {
            client.SendMessage(channel, ".slowoff");
        }

        /// <summary>
        /// Disables slow mode.
        /// </summary>
        /// <param name="channel">String representation of which channel to send slowoff command to.</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void SlowModeOff(this ITwitchClient client, String channel)
        {
            client.SendMessage(channel, ".slowoff");
        }
    }
}
