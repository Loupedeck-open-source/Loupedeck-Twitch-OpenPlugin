﻿namespace TwitchLib.Client.Extensions
{
    using System;
    using Interfaces;
    using Models;

    /// <summary>Extension for implementing clear chat functionality in TwitchClient.</summary>
    public static class ClearChatExt
    {
        /// <summary>
        /// Sends request to clear chat (may be ignored by plugins like BTTV)
        /// </summary>
        /// <param name="channel">JoinedChannel representation of which channel to send clear chat command to.</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void ClearChat(this ITwitchClient client, JoinedChannel channel)
        {
            client.SendMessage(channel, ".clear");
        }

        /// <summary>
        /// Sends request to clear chat (may be ignored by plugins like BTTV)
        /// </summary>
        /// <param name="channel">String representation of which channel to send clear chat command to.</param>
        /// <param name="client">Client reference used to identify extension.</param>
        public static void ClearChat(this ITwitchClient client, String channel)
        {
            client.SendMessage(channel, ".clear");
        }
    }
}
