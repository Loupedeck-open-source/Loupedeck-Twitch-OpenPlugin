namespace Loupedeck.TwitchPlugin.Actions
{
    using System;

    internal class ViewerCountCommand : PluginDynamicCommand
    {
        private const String IMGAction = "TwitchViewers.png";

        private const String InvalidScreenshotFolder = "Shows the current amount of viewers";

        public ViewerCountCommand()
            : base(displayName: "Viewer Count",
                   description: "Shows the current amount of viewers",
                   groupName: "") => this.Name = "ViewerCount";

        protected override Boolean OnLoad()
        {
            TwitchPlugin.Proxy.AppConnected += this.OnAppConnected;
            TwitchPlugin.Proxy.AppDisconnected += this.OnAppDisconnected;
            TwitchPlugin.Proxy.ViewersChanged += this.OnViewersChanged;

            this.IsEnabled = false;
            return true;
        }

        private void OnViewersChanged(Object sender, EventArgs e) => this.ActionImageChanged();

        protected override Boolean OnUnload()
        {
            TwitchPlugin.Proxy.AppConnected -= this.OnAppConnected;
            TwitchPlugin.Proxy.AppDisconnected -= this.OnAppDisconnected;
            TwitchPlugin.Proxy.ViewersChanged -= this.OnViewersChanged;
            return true;
        }

        private void OnAppConnected(Object sender, EventArgs e) => this.IsEnabled = true;

        private void OnAppDisconnected(Object sender, EventArgs e) => this.IsEnabled = false;

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) 
          => (this.Plugin as TwitchPlugin).GetPluginCommandImage(imageSize, IMGAction, TwitchPlugin.Proxy.CurrentViewersCount.ToString());

    }
}
