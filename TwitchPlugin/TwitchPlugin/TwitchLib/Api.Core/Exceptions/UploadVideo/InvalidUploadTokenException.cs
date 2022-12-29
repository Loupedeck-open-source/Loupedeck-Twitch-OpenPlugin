namespace TwitchLib.Api.Core.Exceptions.UploadVideo
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception thrown when the identifying video token is invalid.</summary>
    public class InvalidUploadTokenException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public InvalidUploadTokenException(String apiData)
            : base(apiData)
        {
        }
    }
}
