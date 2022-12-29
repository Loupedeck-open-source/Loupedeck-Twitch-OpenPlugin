namespace TwitchLib.Api.Core.Exceptions.UploadVideo.CompleteUpload
{
    using System;

    /// <inheritdoc />
    /// <summary>Exception thrown attempting to finish an upload without all parts.</summary>
    public class MissingPartsException : Exception
    {
        /// <inheritdoc />
        /// <summary>Exception constructor</summary>
        public MissingPartsException(String apiData)
            : base(apiData)
        {
        }
    }
}
