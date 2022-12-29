namespace TwitchLib.Client.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>Object representing a command received via Twitch chat.</summary>
    public class WhisperCommand
    {
        /// <summary>Property representing all arguments received in a List form.</summary>
        public List<String> ArgumentsAsList { get; }
        /// <summary>Property representing all arguments received in a string form.</summary>
        public String ArgumentsAsString { get; }
        /// <summary>Property representing the command identifier (ie command prefix).</summary>
        public Char CommandIdentifier { get; }
        /// <summary>Property representing the actual command (without the command prefix).</summary>
        public String CommandText { get; }
        /// <summary>Property representing the chat message that the command came in.</summary>
        public WhisperMessage WhisperMessage { get; }

        /// <summary>ChatCommand constructor.</summary>
        /// <param name="whisperMessage"></param>
        public WhisperCommand(WhisperMessage whisperMessage)
        {
            this.WhisperMessage = whisperMessage;
            this.CommandText = whisperMessage.Message.Split(' ')?[0].Substring(1, whisperMessage.Message.Split(' ')[0].Length - 1) ?? whisperMessage.Message.Substring(1, whisperMessage.Message.Length - 1);
            this.ArgumentsAsString = whisperMessage.Message.Replace(whisperMessage.Message.Split(' ')?[0] + " ", "");
            this.ArgumentsAsList = whisperMessage.Message.Split(' ')?.Where(arg => arg != whisperMessage.Message[0] + this.CommandText).ToList() ?? new List<String>();
            this.CommandIdentifier = whisperMessage.Message[0];
        }

        public WhisperCommand(WhisperMessage whisperMessage, String commandText, String argumentsAsString, List<String> argumentsAsList, Char commandIdentifier)
        {
            this.WhisperMessage = whisperMessage;
            this.CommandText = commandText;
            this.ArgumentsAsString = argumentsAsString;
            this.ArgumentsAsList = argumentsAsList;
            this.CommandIdentifier = commandIdentifier;
        }
    }
}
