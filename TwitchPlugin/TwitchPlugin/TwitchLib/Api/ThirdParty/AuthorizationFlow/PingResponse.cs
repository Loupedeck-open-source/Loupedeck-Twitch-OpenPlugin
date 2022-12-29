namespace TwitchLib.Api.ThirdParty.AuthorizationFlow
{
    using System;
    using System.Collections.Generic;
    using Core.Enums;
    using Newtonsoft.Json.Linq;

    public class PingResponse
    {
        public Boolean Success { get; protected set; }
        public String Id { get; protected set; }

        public Int32 Error { get; protected set; }
        public String Message { get; protected set; }

        public List<AuthScopes> Scopes { get; protected set; }
        public String Token { get; protected set; }
        public String Refresh { get; protected set; }
        public String Username { get; protected set; }

        public PingResponse(String jsonStr)
        {
            var json = JObject.Parse(jsonStr);
            this.Success = Boolean.Parse(json.SelectToken("success").ToString());
            if(!this.Success)
            {
                this.Error = Int32.Parse(json.SelectToken("error").ToString());
                this.Message = json.SelectToken("message").ToString();
            } else
            {
                this.Scopes = new List<AuthScopes>();
                foreach (var scope in json.SelectToken("scopes"))
                    this.Scopes.Add(this.StringToScope(scope.ToString()));
                this.Token = json.SelectToken("token").ToString();
                this.Refresh = json.SelectToken("refresh").ToString();
                this.Username = json.SelectToken("username").ToString();
            }
        }
        
        private AuthScopes StringToScope(String scope)
        {
            switch (scope)
            {
                case "user_read":
                    return AuthScopes.User_Read;
                case "user_blocks_edit":
                    return AuthScopes.User_Blocks_Edit;
                case "user_blocks_read":
                    return AuthScopes.User_Blocks_Read;
                case "user_follows_edit":
                    return AuthScopes.User_Follows_Edit;
                case "channel_read":
                    return AuthScopes.Channel_Read;
                case "channel_commercial":
                    return AuthScopes.Channel_Commercial;
                case "channel_stream":
                    return AuthScopes.Channel_Subscriptions;
                case "channel_subscriptions":
                    return AuthScopes.Channel_Subscriptions;
                case "user_subscriptions":
                    return AuthScopes.User_Subscriptions;
                case "channel_check_subscription":
                    return AuthScopes.Channel_Check_Subscription;
                case "chat_login":
                    return AuthScopes.Chat_Login;
                case "channel_feed_read":
                    return AuthScopes.Channel_Feed_Read;
                case "channel_feed_edit":
                    return AuthScopes.Channel_Feed_Edit;
                case "collections_edit":
                    return AuthScopes.Collections_Edit;
                case "communities_edit":
                    return AuthScopes.Communities_Edit;
                case "communities_moderate":
                    return AuthScopes.Communities_Moderate;
                case "viewing_activity_read":
                    return AuthScopes.Viewing_Activity_Read;
                case "user:edit":
                    return AuthScopes.Helix_User_Edit;
                case "user:read:email":
                    return AuthScopes.Helix_User_Read_Email;
                case "clips:edit":
                    return AuthScopes.Helix_Clips_Edit;
                case "analytics:read:games":
                    return AuthScopes.Helix_Analytics_Read_Games;
                case "bits:read":
                    return AuthScopes.Helix_Bits_Read;
                default:
                    throw new Exception("Unknown scope");
            }
        }

    }
}
