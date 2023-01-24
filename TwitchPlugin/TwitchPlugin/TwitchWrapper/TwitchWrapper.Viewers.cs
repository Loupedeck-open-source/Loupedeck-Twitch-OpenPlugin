namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using TwitchLib.Api.Core.Exceptions;
    using System.Linq;

    public partial class TwitchWrapper : IDisposable
    {
        private CancellationTokenSource _viewerUpdaterCancellationTokenSource = new CancellationTokenSource();

        private readonly Object _viewersLock = new Object();
        public EventHandler ViewersChanged { get; set; }

        public Int32 CurrentViewersCount { get; private set; }

        internal void InitViewersUpdater()
        {
            TwitchPlugin.PluginLog.Info("TwitchWrapper InitViewersUpdater");
            this.StopViewersUpdater();
            this._viewerUpdaterCancellationTokenSource = new CancellationTokenSource();
            Task.Run(this.StartUpdateViewersAsync, this._viewerUpdaterCancellationTokenSource.Token);
        }

        internal void StopViewersUpdater()
        {
            TwitchPlugin.PluginLog.Info("TwitchWrapper StopViewersUpdater");

            try
            {
                this._viewerUpdaterCancellationTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                TwitchPlugin.PluginLog.Error(ex, "Error stopping viewers updater");
            }
        }

        private async Task StartUpdateViewersAsync()
        {
            TwitchPlugin.PluginLog.Info("TwitchWrapper StartUpdateViewersAsync");

            try
            {
                while (!this._viewerUpdaterCancellationTokenSource.IsCancellationRequested)
                {
                    await this.UpdateViewersAsync();
                    await Task.Delay(10000);
                }
            }
            catch (ThreadAbortException ex)
            {
                TwitchPlugin.PluginLog.Error(ex, "\nTwitch UpdateViewers stopped");
            }
        }

        private async Task UpdateViewersAsync()
        {
            try
            {
                if (!String.IsNullOrEmpty(this.twitchApi.Settings.AccessToken) && this._twitchClient.IsConnected)
                {
                    this.CurrentViewersCount = await this.GetViewersAsync(this._userInfo.Login);
                    this.ViewersChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex) when (ex is TokenExpiredException || ex is BadScopeException)
            {
                this.StopViewersUpdater();
                this.AccessTokenExpired?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                TwitchPlugin.PluginLog.Error(ex, $"TwitchWrapper UpdateViewersAsync error: {ex.Message}");
            }
        }

        private async Task<Int32> GetViewersAsync(String userLogin)
        {
            var stream = await this.twitchApi.Helix.Streams
                .GetStreamsAsync(first: 1, userLogins: new List<string> { userLogin }) ?? null;
            var currentStream = stream?.Streams?.FirstOrDefault();
            return currentStream?.ViewerCount ?? 0;
        }
    }
}
