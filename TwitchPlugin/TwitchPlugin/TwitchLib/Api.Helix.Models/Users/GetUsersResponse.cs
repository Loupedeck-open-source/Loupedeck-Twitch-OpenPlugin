namespace TwitchLib.Api.Helix.Models.Users
{
    using Newtonsoft.Json;

    public class GetUsersResponse
    {
        [JsonProperty(PropertyName = "data")]
        public User[] Users { get; protected set; }
    }
}
