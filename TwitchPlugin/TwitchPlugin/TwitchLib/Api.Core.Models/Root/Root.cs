namespace TwitchLib.Api.Core.Models.Root
{
    using Newtonsoft.Json;

    public class Root
    {
        #region Token
        /// <summary>Property representing token object.</summary>
        [JsonProperty(PropertyName = "token")]
        public RootToken Token { get; protected set; }
        #endregion
    }
}
