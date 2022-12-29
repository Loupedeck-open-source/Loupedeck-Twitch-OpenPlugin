namespace TwitchLib.Api.Core.Exceptions.UploadVideo
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception representing an internal server error while creating a video.</summary>
    public class InternalServerErrorException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public InternalServerErrorException(String apiData)
            : base(apiData)
        {
        }
    }
}
