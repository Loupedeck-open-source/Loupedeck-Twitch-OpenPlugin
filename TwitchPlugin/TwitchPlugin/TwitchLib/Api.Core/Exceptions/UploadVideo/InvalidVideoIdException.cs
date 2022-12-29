namespace TwitchLib.Api.Core.Exceptions.UploadVideo
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception thrown when the video Id provided is invalid.</summary>
    public class InvalidVideoIdException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public InvalidVideoIdException(String apiData)
            : base(apiData)
        {
        }
    }
}
