namespace Loupedeck.TwitchPlugin.Actions
{
    using System;

    internal class CreateClipCommand : PluginDynamicCommand
    {
        private const String IMGAction = "TwitchCreateClip.png";

        private const String InvalidScreenshotFolder = "Creates clip and displays clip url in chat.";

        public CreateClipCommand()
            : base(displayName: "Create clip",
                   description: "Creates clip and displays clip url in chat.",
                   groupName: "") => this.Name = "CreateClip";

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

        protected override void RunCommand(String actionParameter) => TwitchPlugin.Proxy.AppCreateClipCommand();
    }
}
