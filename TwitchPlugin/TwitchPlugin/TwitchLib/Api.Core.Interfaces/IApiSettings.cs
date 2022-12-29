namespace TwitchLib.Api.Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Enums;

    public interface IApiSettings
    {
        String AccessToken { get; set; }
        String Secret { get; set; }
        String ClientId { get; set; }
        Boolean SkipDynamicScopeValidation { get; set; }
        Boolean SkipAutoServerTokenGeneration { get; set; }
        List<AuthScopes> Scopes { get; set; }

        event PropertyChangedEventHandler PropertyChanged;
    }
}