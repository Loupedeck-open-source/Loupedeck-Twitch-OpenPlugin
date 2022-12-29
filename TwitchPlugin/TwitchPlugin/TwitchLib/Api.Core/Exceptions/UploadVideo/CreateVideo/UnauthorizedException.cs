namespace TwitchLib.Api.Core.Exceptions.UploadVideo.CreateVideo
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception thrown when the passed access token doesn't have the correct scope.</summary>
    public class UnauthorizedException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public UnauthorizedException(String apiData)
            : base(apiData)
        {
        }
    }
}
