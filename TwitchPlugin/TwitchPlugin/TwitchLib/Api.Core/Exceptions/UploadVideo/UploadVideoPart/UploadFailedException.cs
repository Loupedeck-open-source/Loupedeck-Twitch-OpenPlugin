namespace TwitchLib.Api.Core.Exceptions.UploadVideo.UploadVideoPart
{
    using System;

    /// <inheritdoc />
    /// <summary>Thrown when Twitch reports a failure of the upload.</summary>
    public class UploadFailedException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public UploadFailedException(String apiData)
            : base(apiData)
        {
        }
    }
}
