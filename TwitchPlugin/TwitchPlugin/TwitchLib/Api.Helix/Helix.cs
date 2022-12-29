namespace TwitchLib.Api.Helix
{
    using Core;
    using Core.HttpCallHandlers;
    using Core.Interfaces;
    using Core.RateLimiter;
    using Loupedeck.TwitchPlugin;

    public class Helix 
    {
        private readonly ILogger _logger;
        public IApiSettings Settings { get; }
        public Analytics Analytics { get; }
     
        public Bits Bits { get; }
        public Clips Clips { get; }
        public Entitlements Entitlements { get; }
        public Games Games { get; }
        public Streams Streams { get; }
        public Videos Videos { get; }
        public Users Users { get; }
        public Webhooks Webhooks { get; }


        /// <summary>
        /// Creates an Instance of the Helix Class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="rateLimiter">Instance Of RateLimiter, otherwise no ratelimiter is used.</param>
        /// <param name="settings">Instance of ApiSettings, otherwise defaults used, can be changed later</param>
        /// <param name="http">Instance of HttpCallHandler, otherwise default handler used</param>
        public Helix(ILogger logger = null, IRateLimiter rateLimiter = null, IApiSettings settings = null, IHttpCallHandler http = null)
        {
            this._logger = logger;
            rateLimiter = rateLimiter ?? BypassLimiter.CreateLimiterBypassInstance();
            http = http ?? new TwitchHttpClient(logger);
            this.Settings = settings ?? new ApiSettings();

            this.Analytics = new Analytics(this.Settings, rateLimiter, http);
            this.Bits = new Bits(this.Settings, rateLimiter, http);
            this.Clips = new Clips(this.Settings, rateLimiter, http);
            this.Entitlements = new Entitlements(this.Settings, rateLimiter, http);
            this.Games = new Games(this.Settings, rateLimiter, http);
            this.Streams = new Streams(this.Settings, rateLimiter, http);
            this.Users = new Users(this.Settings, rateLimiter, http);
            this.Videos = new Videos(this.Settings, rateLimiter, http);
            this.Webhooks = new Webhooks(this.Settings, rateLimiter, http);
        }
    }
}
