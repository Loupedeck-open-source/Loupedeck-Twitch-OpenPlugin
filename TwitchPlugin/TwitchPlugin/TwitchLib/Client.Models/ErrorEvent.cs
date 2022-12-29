namespace TwitchLib.Client.Models
{
    using System;

    /// <summary>Class representing the error that the websocket encountered.</summary>
    public class ErrorEvent
    {
        /// <summary>Message pertaining to the error.</summary>
        public String Message { get;  set; }        
    }
}
