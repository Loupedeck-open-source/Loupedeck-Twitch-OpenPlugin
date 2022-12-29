namespace TwitchLib.Client.Events
{
    using System;

    /// <inheritdoc />
    /// <summary>Args representing whisper sent event.</summary>
    public class OnWhisperSentArgs : EventArgs
    {
        /// <summary>Property representing username of bot.</summary>
        public String Username;
        /// <summary>Property representing receiver of the whisper.</summary>
        public String Receiver;
        /// <summary>Property representing sent message contents.</summary>
        public String Message;
    }
}
