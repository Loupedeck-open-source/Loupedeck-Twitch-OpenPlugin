namespace TwitchLib.Client.Exceptions
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception thrown when attempting to assign a variable with a different value that is not allowed.</summary>
    public class IllegalAssignmentException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public IllegalAssignmentException(String description)
            : base(description)
        {
        }
    }
}
