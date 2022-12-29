namespace TwitchLib.Client.Events
{
    using System;
    using Exceptions;

    public class OnFailureToReceiveJoinConfirmationArgs : EventArgs
    {
        public FailureToReceiveJoinConfirmationException Exception;
    }
}
