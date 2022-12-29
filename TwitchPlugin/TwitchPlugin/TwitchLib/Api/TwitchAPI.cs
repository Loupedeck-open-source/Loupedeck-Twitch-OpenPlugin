namespace TwitchLib.Api
{
    using System;
    using System.ComponentModel;
    using Core;
    using Core.HttpCallHandlers;
    using Core.Interfaces;
    using Core.RateLimiter;
    using Core.Undocumented;
    using Interfaces;
    using Loupedeck.TwitchPlugin;

    public class TwitchApi : ITwitchAPI
    {
        private readonly ILogger _logger;

        /// <summary>
        ///     Creates an Instance of the TwitchAPI Class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="rateLimiter">Instance Of RateLimiter, otherwise no ratelimiter is used.</param>
        /// <param name="settings">Instance of ApiSettings, otherwise defaults used, can be changed later</param>
        /// <param name="http">Instance of HttpCallHandler, otherwise default handler used</param>
        public TwitchApi(ILogger logger = null, IRateLimiter rateLimiter = null, IApiSettings settings = null,
            IHttpCallHandler http = null)
        {
            this._logger = logger;
            rateLimiter = rateLimiter ?? BypassLimiter.CreateLimiterBypassInstance();
            http = http ?? new TwitchHttpClient(logger);
            this.Settings = settings ?? new ApiSettings();

            this.Helix = new Helix.Helix(logger, rateLimiter, this.Settings, http);
            this.ThirdParty = new ThirdParty.ThirdParty(this.Settings, rateLimiter, http);
            this.Undocumented = new Undocumented(this.Settings, rateLimiter, http);

            this.Settings.PropertyChanged += this.SettingsPropertyChanged;
        }

        public IApiSettings Settings { get; }
        public Helix.Helix Helix { get; }
        public ThirdParty.ThirdParty ThirdParty { get; }
        public Undocumented Undocumented { get; }

        private void SettingsPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IApiSettings.AccessToken):
                    this.Helix.Settings.AccessToken = this.Settings.AccessToken;
                    break;
                case nameof(IApiSettings.Secret):
                    this.Helix.Settings.Secret = this.Settings.Secret;
                    break;
                case nameof(IApiSettings.ClientId):
                    this.Helix.Settings.ClientId = this.Settings.ClientId;
                    break;
                case nameof(IApiSettings.SkipDynamicScopeValidation):
                    this.Helix.Settings.SkipDynamicScopeValidation = this.Settings.SkipDynamicScopeValidation;
                    break;
                case nameof(IApiSettings.SkipAutoServerTokenGeneration):
                    this.Helix.Settings.SkipAutoServerTokenGeneration = this.Settings.SkipAutoServerTokenGeneration;
                    break;
                case nameof(IApiSettings.Scopes):
                    this.Helix.Settings.Scopes = this.Settings.Scopes;
                    break;
            }
        }
    }
}