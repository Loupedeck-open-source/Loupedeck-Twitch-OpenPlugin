namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    /// <inheritdoc />
    /// <summary></summary>
    public class OnWhisperReceivedArgs : EventArgs
    {
        /// <summary></summary>
        public WhisperMessage WhisperMessage;
    }
}
