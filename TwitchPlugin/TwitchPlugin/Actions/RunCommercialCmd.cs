namespace Loupedeck.TwitchPlugin.Actions
{
    using System;

    public class RunCommercialCmd : ActionEditorCommand
    {
        private const String ListboxControlName = "dura";
        private const String IMGAction = "TwitchAd1.png";

        private static readonly Int32[] _durations = { 30, 60, 90, 120, 150, 180 };

        public RunCommercialCmd()
        {
            this.DisplayName = "Run Commercial";
            this.Description = "Runs a commercial during your Twitch stream (Partners only)";

            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(name: ListboxControlName, labelText: "Seconds:"));

            this.ActionEditor.ListboxItemsRequested += this.OnActionEditorListboxItemsRequested;
            this.ActionEditor.ControlValueChanged += this.OnActionEditorControlValueChanged;

        }

        protected override Boolean OnLoad()
        {
            TwitchPlugin.Proxy.AppConnected += this.OnAppConnected;
            TwitchPlugin.Proxy.AppDisconnected += this.OnAppDisconnected;

            return true;
        }

        protected override Boolean OnUnload()
        {
            TwitchPlugin.Proxy.AppConnected -= this.OnAppConnected;
            TwitchPlugin.Proxy.AppDisconnected -= this.OnAppDisconnected;

            return true;
        }

        private void OnActionEditorControlValueChanged(Object sender, ActionEditorControlValueChangedEventArgs e)
        {
            if (e.ControlName.EqualsNoCase(ListboxControlName))
            {
                e.ActionEditorState.SetDisplayName($"Run commercial for {e.ActionEditorState.GetControlValue(ListboxControlName)}s");
            }
        }
        private void OnActionEditorListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
                  => ActionHelpers.FillListBox(e, ListboxControlName, () => Array.ForEach(_durations, (x) 
                      => e.AddItem(name: $"{x}", displayName: $"Run commercial for {x}s", description: null)));

        private void OnAppConnected(Object sender, EventArgs e)
        {
        }

        private void OnAppDisconnected(Object sender, EventArgs e)
        {
        }

        protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 imageWidth, Int32 imageHeight)
               => (this.Plugin as TwitchPlugin).GetPluginCommandImage(imageWidth, imageHeight, IMGAction,
                        actionParameters.TryGetString(ListboxControlName, out var cmdDuration)
                            ? cmdDuration
                            : "n/a");
        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            return actionParameters.TryGetString(ListboxControlName, out var cmdDuration)
                   && TwitchPlugin.Proxy.AppRunCommercial(Int32.TryParse(cmdDuration, out var duration) ? duration : 30);
        }
    }
}
