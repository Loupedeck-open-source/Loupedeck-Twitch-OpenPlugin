namespace TwitchLib.Api.Core.Exceptions.UploadVideo.CreateVideo
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception thrown when attempting to upload to an invalid channel.</summary>
    public class InvalidChannelException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public InvalidChannelException(String apiData)
            : base(apiData)
        {
        }
    }
}
