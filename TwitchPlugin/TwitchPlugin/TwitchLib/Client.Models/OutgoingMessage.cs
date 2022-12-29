namespace TwitchLib.Client.Models
{
    using System;

    public class OutgoingMessage
    {
        public String Channel { get; set; }
        public String Message { get; set; }
	public Int32 Nonce { get; set; }
        public String Sender { get; set; }
        public MessageState State { get; set; }        
    }

    public enum MessageState : byte
    {
        /// <summary> Message did not originate from this session, or was successfully sent. </summary>
		Normal = 0,
        /// <summary> Message is current queued. </summary>
		Queued,
        /// <summary> Message failed to be sent. </summary>
		Failed
    }
}
 
