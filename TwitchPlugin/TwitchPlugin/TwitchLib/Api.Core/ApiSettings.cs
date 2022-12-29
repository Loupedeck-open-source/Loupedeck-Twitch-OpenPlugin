namespace TwitchLib.Api.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Enums;
    using Interfaces;

    public class ApiSettings : IApiSettings, INotifyPropertyChanged
    {
        private String _clientId;
        private String _secret;
        private String _accessToken;
        private Boolean _skipDynamicScopeValidation;
        private Boolean _skipAutoServerTokenGeneration;
        private List<AuthScopes> _scopes;
        public String ClientId
        {
            get => this._clientId;
            set
            {
                if (value != this._clientId)
                {
                    this._clientId = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public String Secret
        {
            get => this._secret;
            set
            {
                if (value != this._secret)
                {
                    this._secret = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public String AccessToken
        {
            get => this._accessToken;
            set
            {
                if (value != this._accessToken)
                {
                    this._accessToken = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public Boolean SkipDynamicScopeValidation
        {
            get => this._skipDynamicScopeValidation;
            set
            {
                if (value != this._skipDynamicScopeValidation)
                {
                    this._skipDynamicScopeValidation = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public Boolean SkipAutoServerTokenGeneration
        {
            get => this._skipAutoServerTokenGeneration;
            set
            {
                if (value != this._skipAutoServerTokenGeneration)
                {
                    this._skipAutoServerTokenGeneration = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public List<AuthScopes> Scopes
        {
            get => this._scopes;
            set
            {
                if (value != this._scopes)
                {
                    this._scopes = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
