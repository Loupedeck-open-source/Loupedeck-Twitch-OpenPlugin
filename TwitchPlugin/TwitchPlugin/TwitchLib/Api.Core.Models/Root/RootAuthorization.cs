namespace TwitchLib.Api.Core.Models.Root
{
    using System;
    using Newtonsoft.Json;

    public class RootAuthorization
    {
        #region CreatedAt
        /// <summary>Property representing the date time of channel creation.</summary>
        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; protected set; }
        #endregion
        #region Scopes
        /// <summary>Property representing the scopes.</summary>
        [JsonProperty(PropertyName = "scopes")]
        public String[] Scopes { get; protected set; }
        #endregion
        #region UpdatedAt
        /// <summary>Property representing the date time of last channel update.</summary>
        [JsonProperty(PropertyName = "updated_at")]
        public DateTime UpdatedAt { get; protected set; }
        #endregion
    }
}
