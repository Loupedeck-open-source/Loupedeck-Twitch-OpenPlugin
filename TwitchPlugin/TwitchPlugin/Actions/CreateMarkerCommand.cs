namespace Loupedeck.TwitchPlugin.Actions
{
    using System;

    internal class CreateMarkerCommand : PluginDynamicCommand
    {
        private const String IMGAction = "CreateMarker.svg";

        private const String InvalidScreenshotFolder = "Creates a marker in the stream.";

        public CreateMarkerCommand()
            : base(displayName: "Create marker",
                   description: "Creates a marker in the stream.",
                   groupName: "") => this.Name = "CreateMarker";

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

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) => EmbeddedResources.ReadBinaryFile(TwitchPlugin.ImageResPrefix + IMGAction).ToImage();

        protected override void RunCommand(String actionParameter) => TwitchPlugin.Proxy.CreateMarkerCommand();
    }
}
