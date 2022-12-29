namespace TwitchLib.Client.Events
{
    using System;
    using Models;

    /// <inheritdoc />
    /// <summary>Args representing whisper command received event.</summary>
    public class OnWhisperCommandReceivedArgs : EventArgs
    {
        /// <summary>Property representing received command.</summary>
        public WhisperCommand Command;
    }
}
