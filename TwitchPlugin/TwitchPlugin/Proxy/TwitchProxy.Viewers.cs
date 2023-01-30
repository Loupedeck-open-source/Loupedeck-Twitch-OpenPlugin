namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using TwitchLib.Api.Core.Exceptions;
    using System.Linq;
    using TwitchLib.Communication.Events;
    using TwitchLib.Client.Events;

    public partial class TwitchProxy : IDisposable
    {
        private readonly Object _viewersLock = new Object();

        public event EventHandler<EventArgs> ViewersChanged;

        private readonly System.Timers.Timer _viewersUpdatetimer = null;
        private Int32 _currentViewersCount;

        public Int32 CurrentViewersCount 
        {
            get
            {
                return this._currentViewersCount;
            }

            private set
            {
                if(this._currentViewersCount != value)
                { 
                    this._currentViewersCount = value;
                    this.ViewersChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void OnViewersUpdateTimerTick(Object _, Object _1)
        {
            try
            {
                var viewers = 0; 
                if (this.IsConnected)
                {
                    var response = this.twitchApi.Helix.Streams.GetStreamsAsync(first: 1, userLogins: new List<String> { this._userInfo.Login }).Result;
                    TwitchPlugin.PluginLog.Info("Updating viewers");
                    if((response != null) && (response.Streams.Count() > 0)) 
                    {
                        viewers = response.Streams[0].ViewerCount;
                    }
                }
                this.CurrentViewersCount = viewers;
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    if (e is TokenExpiredException || e is BadScopeException)
                    {
                        //For whatever reason we might be those getting this first...
                        TwitchPlugin.PluginLog.Error(e, $"OnViewersUpdateTimerTick Aggregate error: {e.Message}");
                        this.OnTwitchAccessTokenExpired?.BeginInvoke(this, EventArgs.Empty);
                        return true; // exception was handled
                    }
                    return false; // exception was not handled
                });
            }
            catch (Exception ex) when (ex is TokenExpiredException || ex is BadScopeException)
            {
                //For whatever reason we might be those getting this first...
                TwitchPlugin.PluginLog.Error(ex, $"OnViewersUpdateTimerTick expired error: {ex.Message}");
                this.OnTwitchAccessTokenExpired?.BeginInvoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                //TODO: Figure out 
                TwitchPlugin.PluginLog.Error(ex, $"OnViewersUpdateTimerTick unhandled error: {ex.Message}");
            }
        }

        private void StartViewersUpdateTimer(Object sender, TwitchLib.Client.Events.OnConnectedArgs e)
        {
            //We're connected
            TwitchPlugin.PluginLog.Info($"Enabling Viewers update timer");
            this._viewersUpdatetimer.Enabled = true;
        }
            

        private void StopViewersUpdateTimer(Object sender, OnDisconnectedEventArgs e)
        {
            TwitchPlugin.PluginLog.Info($"Disabling Viewers update timer");
            this._viewersUpdatetimer.Enabled = true;
        }
        
    }
}
