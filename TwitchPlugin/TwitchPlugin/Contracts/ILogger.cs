namespace Loupedeck.TwitchPlugin
{
    using System;

    public interface ILogger
    {
        void LogInformation(String message, params Object[] args);

        void LogError(String message, params Object[] args);
    }
}