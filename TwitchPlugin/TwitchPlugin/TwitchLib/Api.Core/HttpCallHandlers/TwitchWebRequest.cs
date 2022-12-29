namespace TwitchLib.Api.Core.HttpCallHandlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using Common;
    using Enums;
    using Exceptions;
    using Interfaces;
    using Loupedeck.TwitchPlugin;

    public class TwitchWebRequest : IHttpCallHandler
    {
        private readonly ILogger _logger;

        /// <summary>
        ///     Creates an Instance of the TwitchHttpClient Class.
        /// </summary>
        /// <param name="logger">Instance Of Logger, otherwise no logging is used,  </param>
        public TwitchWebRequest(ILogger logger = null)
        {
            this._logger = logger;
        }


        public void PutBytes(String url, Byte[] payload)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.UploadData(new Uri(url), "PUT", payload);
                }
            }
            catch (WebException ex)
            {
                this.HandleWebException(ex);
            }
        }

        public KeyValuePair<Int32, String> GeneralRequest(String url, String method, String payload = null,
            ApiVersion api = ApiVersion.V5, String clientId = null, String accessToken = null)
        {
            var request = WebRequest.CreateHttp(url);
            if (String.IsNullOrEmpty(clientId) && String.IsNullOrEmpty(accessToken))
                throw new InvalidCredentialException(
                    "A Client-Id or OAuth token is required to use the Twitch API. If you previously set them in InitializeAsync, please be sure to await the method.");


            if (!String.IsNullOrEmpty(clientId)) request.Headers["Client-ID"] = clientId;


            request.Method = method;
            request.ContentType = "application/json";

            var authPrefix = "OAuth";
            if (api == ApiVersion.Helix)
            {
                request.Accept = "application/json";
                authPrefix = "Bearer";
            }
            else if (api != ApiVersion.Void)
            {
                request.Accept = $"application/vnd.twitchtv.v{(Int32) api}+json";
            }

            if (!String.IsNullOrEmpty(accessToken))
                request.Headers["Authorization"] = $"{authPrefix} {Helpers.FormatOAuth(accessToken)}";


            if (payload != null)
                using (var writer = new StreamWriter(request.GetRequestStreamAsync().GetAwaiter().GetResult()))
                {
                    writer.Write(payload);
                }

            try
            {
                var response = (HttpWebResponse) request.GetResponse();

                using (var reader =
                    new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()))
                {
                    var data = reader.ReadToEnd();
                    return new KeyValuePair<Int32, String>((Int32) response.StatusCode, data);
                }
            }
            catch (WebException ex)
            {
                this.HandleWebException(ex);
            }

            return new KeyValuePair<Int32, String>(0, null);
        }

        public Int32 RequestReturnResponseCode(String url, String method,
            List<KeyValuePair<String, String>> getParams = null)
        {
            if (getParams != null)
                for (var i = 0; i < getParams.Count; i++)
                    if (i == 0)
                        url += $"?{getParams[i].Key}={Uri.EscapeDataString(getParams[i].Value)}";
                    else
                        url += $"&{getParams[i].Key}={Uri.EscapeDataString(getParams[i].Value)}";

            var req = (HttpWebRequest) WebRequest.Create(url);
            req.Method = method;
            var response = (HttpWebResponse) req.GetResponse();
            return (Int32) response.StatusCode;
        }

        private void HandleWebException(WebException e)
        {
            if (!(e.Response is HttpWebResponse errorResp))
                throw e;
            switch (errorResp.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    throw new BadRequestException(
                        "Your request failed because either: \n 1. Your ClientID was invalid/not set. \n 2. Your refresh token was invalid. \n 3. You requested a username when the server was expecting a user ID.");
                case HttpStatusCode.Unauthorized:
                    var authenticateHeader = errorResp.Headers.GetValues("WWW-Authenticate");
                    if (authenticateHeader?.Length == 0 || String.IsNullOrEmpty(authenticateHeader?[0]))
                        throw new BadScopeException(
                            "Your request was blocked due to bad credentials (do you have the right scope for your access token?).");

                    var invalidTokenFound = authenticateHeader[0].Contains("error='invalid_token'");
                    if (invalidTokenFound)
                        throw new TokenExpiredException(
                            "Your request was blocked du to an expired Token. Please refresh your token and update your API instance settings.");
                    break;
                case HttpStatusCode.NotFound:
                    throw new BadResourceException("The resource you tried to access was not valid.");
                case (HttpStatusCode) 429:
                    var resetTime = errorResp.Headers.Get("Ratelimit-Reset");
                    throw new TooManyRequestsException("You have reached your rate limit. Too many requests were made",
                        resetTime);
                case (HttpStatusCode) 422:
                    throw new NotPartneredException(
                        "The resource you requested is only available to channels that have been partnered by Twitch.");
                default:
                    throw e;
            }
        }
    }
}