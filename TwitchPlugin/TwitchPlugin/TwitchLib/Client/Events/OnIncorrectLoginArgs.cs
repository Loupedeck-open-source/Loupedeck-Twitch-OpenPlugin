namespace TwitchLib.Client.Events
{
    using System;
    using Exceptions;

    /// <inheritdoc />
    /// <summary>Args representing an incorrect login event.</summary>
    public class OnIncorrectLoginArgs : EventArgs
    {
        /// <summary>Property representing exception object.</summary>
        public ErrorLoggingInException Exception;
    }
}
