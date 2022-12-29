using Loupedeck;

namespace TwitchLib.Api.Core.HttpCallHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using Common;
    using Enums;
    using Exceptions;
    using Interfaces;
    using Internal;
    using Loupedeck.TwitchPlugin;

    public class TwitchHttpClient : IHttpCallHandler
    {
        private readonly HttpClient _http;
        private readonly ILogger _logger;

        /// <summary>
        ///     Creates an Instance of the TwitchHttpClient Class.
        /// </summary>
        /// <param name="logger">Instance Of Logger, otherwise no logging is used,  </param>
        public TwitchHttpClient(ILogger logger = null)
        {
            this._logger = logger;
            this._http = new HttpClient(new TwitchHttpClientHandler(this._logger));
        }


        public void PutBytes(String url, Byte[] payload)
        {
            var response = this._http.PutAsync(new Uri(url), new ByteArrayContent(payload)).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode) this.HandleWebException(response);
        }

        public KeyValuePair<Int32, String> GeneralRequest(String url, String method, String payload = null,
            ApiVersion api = ApiVersion.V5, String clientId = null, String accessToken = null)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = new HttpMethod(method)
            };

            if (String.IsNullOrEmpty(clientId) && String.IsNullOrEmpty(accessToken))
                throw new InvalidCredentialException(
                    "A Client-Id or OAuth token is required to use the Twitch API. If you previously set them in InitializeAsync, please be sure to await the method.");

            if (!String.IsNullOrEmpty(clientId)) request.Headers.Add("Client-ID", clientId);

            var authPrefix = "OAuth";
            if (api == ApiVersion.Helix)
            {
                request.Headers.Add(HttpRequestHeader.Accept.ToString(), "application/json");
                authPrefix = "Bearer";
            }
            else if (api != ApiVersion.Void)
            {
                request.Headers.Add(HttpRequestHeader.Accept.ToString(),
                    $"application/vnd.twitchtv.v{(Int32) api}+json");
            }

            if (!String.IsNullOrEmpty(accessToken))
                request.Headers.Add(HttpRequestHeader.Authorization.ToString(),
                    $"{authPrefix} {Helpers.FormatOAuth(accessToken)}");

            if (payload != null)
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            try
            {
                var response = this._http.SendAsync(request).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    var respStr = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    return new KeyValuePair<Int32, String>((Int32)response.StatusCode, respStr);
                }
                this.HandleWebException(response);
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Warning(e,"Twitch client obtained an arror during sending request");
                throw;
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

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = new HttpMethod(method)
            };
            var response = this._http.SendAsync(request).GetAwaiter().GetResult();
            return (Int32) response.StatusCode;
        }

        private void HandleWebException(HttpResponseMessage errorResp)
        {
            switch (errorResp.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    throw new BadRequestException(
                        "Your request failed because either: \n 1. Your ClientID was invalid/not set. \n 2. Your refresh token was invalid. \n 3. You requested a username when the server was expecting a user ID.");
                case HttpStatusCode.Unauthorized:
                    var authenticateHeader = errorResp.Headers.WwwAuthenticate;
                    if (authenticateHeader == null || authenticateHeader.Count <= 0)
                        throw new BadScopeException(
                            "Your request was blocked due to bad credentials (Do you have the right scope for your access token?).");
                    else
                        throw new TokenExpiredException(
                            "Your request was blocked due to an expired Token. Please refresh your token and update your API instance settings.");
                case HttpStatusCode.NotFound:
                    throw new BadResourceException("The resource you tried to access was not valid.");
                case (HttpStatusCode) 422:
                    throw new NotPartneredException(
                        "The resource you requested is only available to channels that have been partnered by Twitch.");
                case (HttpStatusCode) 429:
                    errorResp.Headers.TryGetValues("Ratelimit-Reset", out var resetTime);
                    throw new TooManyRequestsException("You have reached your rate limit. Too many requests were made",
                        resetTime.FirstOrDefault());
                case HttpStatusCode.BadGateway:
                    throw new BadGatewayException("The API answered with a 502 Bad Gateway. Please retry your request");
                case HttpStatusCode.GatewayTimeout:
                    throw new GatewayTimeoutException(
                        "The API answered with a 504 Gateway Timeout. Please retry your request");
                case HttpStatusCode.InternalServerError:
                    throw new InternalServerErrorException(
                        "The API answered with a 500 Internal Server Error. Please retry your request");
                default:
                    throw new HttpRequestException("Something went wrong during the request! Please try again later");
            }
        }
    }
}