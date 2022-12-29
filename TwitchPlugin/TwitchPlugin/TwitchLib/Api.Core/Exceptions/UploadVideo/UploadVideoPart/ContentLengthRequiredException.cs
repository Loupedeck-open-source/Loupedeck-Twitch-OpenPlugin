namespace TwitchLib.Api.Core.Exceptions.UploadVideo.UploadVideoPart
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception thrown when a content-length is missing from the upload request.</summary>
    public class ContentLengthRequiredException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public ContentLengthRequiredException(String apiData)
            : base(apiData)
        {
        }
    }
}
