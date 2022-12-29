namespace TwitchLib.Api.Core.Models.Root
{
    using System;
    using Newtonsoft.Json;

    public class RootToken
    {
        #region Authorization
        /// <summary>Property representing authorization object.</summary>
        [JsonProperty(PropertyName = "authorization")]
        public RootAuthorization Auth { get; protected set; }
        #endregion
        #region ClientId
        /// <summary>Property representing the Client ID.</summary>
        [JsonProperty(PropertyName = "client_id")]
        public String ClientId { get; protected set; }
        #endregion
        #region UserId
        /// <summary>Property representing the userId.</summary>
        [JsonProperty(PropertyName = "user_id")]
        public String UserId { get; protected set; }
        #endregion
        #region Username
        /// <summary>Property representing the username.</summary>
        [JsonProperty(PropertyName = "user_name")]
        public String Username { get; protected set; }
        #endregion
        #region Valid
        /// <summary>Property representing if the auth token is valid.</summary>
        [JsonProperty(PropertyName = "valid")]
        public Boolean Valid { get; protected set; }
        #endregion
    }
}
