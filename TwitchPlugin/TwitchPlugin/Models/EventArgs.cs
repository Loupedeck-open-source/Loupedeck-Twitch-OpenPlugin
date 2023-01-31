namespace Loupedeck.TwitchPlugin
{
    using System;
    using TwitchLib.Api.Auth;

    public class TimeSpanEventArg : EventArgs
    {
        public Int32 SlowModeRange { get; private set; }
        public TimeSpanEventArg(Int32 seconds) => this.SlowModeRange = seconds;
    }

    public class AccessTokenReceivedEventArgs : EventArgs
    {
        public String AccessToken { get; private set; }
        public String RefreshToken { get; private set; }
        public String Login { get; protected set; }
        public String UserId { get; protected set; }
        public Int32 ExpiresIn { get; protected set; }


        public AccessTokenReceivedEventArgs(String accessToken, String refreshToken, ValidateAccessTokenResponse validation)
        {
            this.AccessToken = accessToken;
            this.RefreshToken = refreshToken;
            this.Login = validation.Login;
            this.UserId = validation.UserId;
            this.ExpiresIn = validation.ExpiresIn;
        }

        public AccessTokenReceivedEventArgs(String accessToken, String refreshToken, String userId, String login, Int32 expiresIn)
        {
            this.AccessToken = accessToken;
            this.RefreshToken = refreshToken;
            this.Login = login;
            this.UserId = userId;
            this.ExpiresIn = expiresIn;
        }
    }

    public class AccessTokenErrorEventArgs : EventArgs
    {
        public enum TokenError
        {
            ERROR_SECRET,
            ERROR_VALIDATION,
            ERROR_MISC
        };

        public TokenError errorCode { get; private set; }

        public AccessTokenErrorEventArgs(TokenError _err) => this.errorCode = _err;

    }


    public class TokensUpdatedEventArg : EventArgs
    {
        public String UserName { get; private set; }
        public String AccessToken { get; private set; }
        public String RefreshToken { get; private set; }
        public TokensUpdatedEventArg(String _userName, String _accessToken, String _refreshToken)
        {
            this.UserName = _userName;
            this.AccessToken = _accessToken;
            this.RefreshToken = _refreshToken;
        }
    }




}