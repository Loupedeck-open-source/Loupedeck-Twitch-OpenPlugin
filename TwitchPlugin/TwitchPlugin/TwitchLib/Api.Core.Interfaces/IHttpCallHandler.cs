namespace TwitchLib.Api.Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Enums;

    public interface IHttpCallHandler
    {
        KeyValuePair<Int32, String> GeneralRequest(String url, String method, String payload = null,
            ApiVersion api = ApiVersion.V5, String clientId = null, String accessToken = null);

        void PutBytes(String url, Byte[] payload);
        Int32 RequestReturnResponseCode(String url, String method, List<KeyValuePair<String, String>> getParams = null);
    }
}