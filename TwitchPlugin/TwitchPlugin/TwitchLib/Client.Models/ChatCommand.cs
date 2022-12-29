namespace TwitchLib.Client.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>Object representing a command received via Twitch chat.</summary>
    public class ChatCommand
    {
        /// <summary>Property representing all arguments received in a List form.</summary>
        public List<String> ArgumentsAsList { get; }
        /// <summary>Property representing all arguments received in a string form.</summary>
        public String ArgumentsAsString { get; }
        /// <summary>Property representing the chat message that the command came in.</summary>
        public ChatMessage ChatMessage { get; }
        /// <summary>Property representing the command identifier (ie command prefix).</summary>
        public Char CommandIdentifier { get; }
        /// <summary>Property representing the actual command (without the command prefix).</summary>
        public String CommandText { get; }
        

        /// <summary>ChatCommand constructor.</summary>
        /// <param name="chatMessage"></param>
        public ChatCommand(ChatMessage chatMessage)
        {
            this.ChatMessage = chatMessage;
            this.CommandText = chatMessage.Message.Split(' ')?[0].Substring(1, chatMessage.Message.Split(' ')[0].Length - 1) ?? chatMessage.Message.Substring(1, chatMessage.Message.Length - 1); ;
            this.ArgumentsAsString = chatMessage.Message.Contains(" ") ? chatMessage.Message.Replace(chatMessage.Message.Split(' ')?[0] + " ", "") : "";
            if (!chatMessage.Message.Contains("\"") || chatMessage.Message.Count(x => x == '"') % 2 == 1)
                this.ArgumentsAsList = chatMessage.Message.Split(' ')?.Where(arg => arg != chatMessage.Message[0] + this.CommandText).ToList() ?? new List<String>();
            else
                this.ArgumentsAsList = Common.Helpers.ParseQuotesAndNonQuotes(this.ArgumentsAsString);
            this.CommandIdentifier = chatMessage.Message[0];
        }

        public ChatCommand(ChatMessage chatMessage, String commandText, String argumentsAsString, List<String> argumentsAsList, Char commandIdentifier)
        {
            this.ChatMessage = chatMessage;
            this.CommandText = commandText;
            this.ArgumentsAsString = argumentsAsString;
            this.ArgumentsAsList = argumentsAsList;
            this.CommandIdentifier = commandIdentifier;
        }
    }
}
