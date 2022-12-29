namespace TwitchLib.Communication.Models
{
    using System;
    using Enums;
    using Interfaces;

    public class ClientOptions : IClientOptions
    {
        public Int32 SendQueueCapacity { get; set; } = 10000;
        public TimeSpan SendCacheItemTimeout { get; set; } = TimeSpan.FromMinutes(30);
        public UInt16 SendDelay { get; set; } = 50;
        public ReconnectionPolicy ReconnectionPolicy { get; set; } = new ReconnectionPolicy(3000, maxAttempts: 10);
        public Boolean UseSsl { get; set; } = true;
        public Int32 DisconnectWait { get; set; } = 20000;
        public ClientType ClientType { get; set; } = ClientType.Chat;
        public TimeSpan ThrottlingPeriod { get; set; } = TimeSpan.FromSeconds(30);
        public Int32 MessagesAllowedInPeriod { get; set; } = 100;
        public TimeSpan WhisperThrottlingPeriod { get; set; } = TimeSpan.FromSeconds(60);
        public Int32 WhispersAllowedInPeriod { get; set; } = 100;
        public Int32 WhisperQueueCapacity { get; set; } = 10000;
    }
}
