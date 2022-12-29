namespace Loupedeck.TwitchPlugin
{
    using System;

    public interface IAuthenticationServer: IDisposable
    {
        EventHandler<Token> TokenReceived { get; set; }
        void Start(TwitchPluginConfig config);
        void Authenticate();
        void RefreshAccessToken(String refreshToken);
        Boolean IsTokenValid(String accessToken);
    }
}