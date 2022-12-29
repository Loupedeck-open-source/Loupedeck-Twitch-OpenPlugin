﻿namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    /// <inheritdoc />
    /// <summary>Args representing message sent event.</summary>
    public class OnMessageSentArgs : EventArgs
    {
        /// <summary>Property representing a chat message that was just sent (check null on properties before using).</summary>
        public SentMessage SentMessage;
    }
}
