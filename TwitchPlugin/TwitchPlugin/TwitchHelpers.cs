namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.IO;
    using System.Net;
    using Newtonsoft.Json;

    public class TwitchHelpers
    {
        public static TokenInfo GetTokenInfo(String token)
        {
            var validateRequest = (HttpWebRequest)WebRequest.Create("https://id.twitch.tv/oauth2/validate");
            validateRequest.Method = "GET";
            validateRequest.Headers.Add("Authorization", $"OAuth {token}");
            try
            {
                var response = validateRequest.GetResponse();
                var stream = response.GetResponseStream();
                var reader = new StreamReader(stream);
                var responseFromServer = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<TokenInfo>(responseFromServer);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("401"))
                {
                    throw new InvalidAccessTokenException();
                }

                TwitchPlugin.PluginLog.Error(e, e.Message);
                throw;
            }
        }
    }
}