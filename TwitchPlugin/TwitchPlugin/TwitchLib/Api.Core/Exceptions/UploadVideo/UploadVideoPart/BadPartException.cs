namespace TwitchLib.Api.Core.Exceptions.UploadVideo.UploadVideoPart
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception thrown when this library detects the part is invalid.</summary>
    public class BadPartException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public BadPartException(String apiData)
            : base(apiData)
        {
        }
    }
}
