namespace TwitchLib.Client.Models
{
    using System;

    /// <summary>Class representing a joined channel.</summary>
    public class JoinedChannel
    {        
        /// <summary>The current channel the TwitcChatClient is connected to.</summary>
        public String Channel { get; }
        /// <summary>Object representing current state of channel (r9k, slow, etc).</summary>
        public ChannelState ChannelState { get; protected set; }
        /// <summary>The most recent message received.</summary>
        public ChatMessage PreviousMessage { get; protected set; }

        /// <summary>JoinedChannel object constructor.</summary>
        public JoinedChannel(String channel)
        {
            this.Channel = channel;
        }

        /// <summary>Handles a message</summary>
        public void HandleMessage(ChatMessage message)
        {
            this.PreviousMessage = message;
        }
    }
}
