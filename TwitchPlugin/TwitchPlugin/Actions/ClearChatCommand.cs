namespace Loupedeck.TwitchPlugin.Actions
{
    using System;

    internal class ClearChatCommand : PluginDynamicCommand
    {
        private const String IMGAction = "TwitchClearChat1.png";

        private const String InvalidScreenshotFolder = "Clears all messages in Twitch chat";

        public ClearChatCommand()
            : base(displayName: "Clear chat",
                   description: "Clears all messages in Twitch chat",
                   groupName: "") => this.Name = "ClearChat";

        protected override Boolean OnLoad()
        {
            TwitchPlugin.Proxy.AppConnected += this.OnAppConnected;
            TwitchPlugin.Proxy.AppDisconnected += this.OnAppDisconnected;
            this.IsEnabled = false;
            return true;
        }

        protected override Boolean OnUnload()
        {
            TwitchPlugin.Proxy.AppConnected -= this.OnAppConnected;
            TwitchPlugin.Proxy.AppDisconnected -= this.OnAppDisconnected;
            return true;
        }

        private void OnAppConnected(Object sender, EventArgs e) => this.IsEnabled = true;

        private void OnAppDisconnected(Object sender, EventArgs e) => this.IsEnabled = false;

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => (this.Plugin as TwitchPlugin).GetPluginCommandImage(imageSize, IMGAction);

        protected override void RunCommand(String actionParameter) => TwitchPlugin.Proxy.AppClearChat();
    }
}
