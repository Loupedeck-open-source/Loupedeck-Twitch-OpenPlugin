namespace Loupedeck.TwitchPlugin.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http.Headers;

    internal class ChatSlowModeCommand : MultistateActionEditorCommand
    {
        private static readonly Int32[] SlowModeTimeSpans = new Int32[] {  3, 5, 10, 20, 30, 60, 120 };

        private static readonly Dictionary<String, String> _allSlowCommandsActionParameters = new Dictionary<String, String>()
        {
            [SlowModeDurationControl] = "3",
            [SlowModeDurationControl] = "5",
            [SlowModeDurationControl] = "10",
            [SlowModeDurationControl] = "20",
            [SlowModeDurationControl] = "30",
            [SlowModeDurationControl] = "60",
            [SlowModeDurationControl] = "120"
        };

        private readonly String ImgOn  = "TwitchSlowChat.png";
        private readonly String ImgOff = "TwitchSlowChatToggle.png";

        private const Int32 STATE_OFF = 0;
        private const Int32 STATE_ON  = 1;

        private const String SlowModeDurationControl = "slowModeDuration";
        private const String OnStateName = "ON";
        private const String OffStateName = "OFF";


        public ChatSlowModeCommand()
        {
            this.DisplayName = "Chat Slow mode";
            this.Description = "Require chat users to wait between sending messages. Subscribers can be exempted via Partner/Affiliate settings";
            this.GroupName = "";
            this.Name = "ToggleSlowChatList";

            this.ActionEditor.AddControl(
                           new ActionEditorListbox(name: SlowModeDurationControl, labelText: "Slow mode duration:"));

            this.ActionEditor.ListboxItemsRequested += this.OnActionEditorListboxItemsRequested;
            this.ActionEditor.ControlValueChanged += this.OnActionEditorControlValueChanged;

            this.AddState(OnStateName, "Slow Mode on");
            this.AddState(OffStateName, "Slow Mode off");
        }

        protected override Boolean OnLoad()
        {
            TwitchPlugin.Proxy.AppConnected += this.OnAppConnected;
            TwitchPlugin.Proxy.AppDisconnected += this.OnAppDisconnected;
            TwitchPlugin.Proxy.AppEvtChatSlowModeOn += this.OnAppSlowModeOn;
            TwitchPlugin.Proxy.AppEvtChatSlowModeOff += this.OnAppSlowModeOff;
            return true;
        }

        protected override Boolean OnUnload()
        {
            TwitchPlugin.Proxy.AppConnected -= this.OnAppConnected;
            TwitchPlugin.Proxy.AppDisconnected -= this.OnAppDisconnected;
            TwitchPlugin.Proxy.AppEvtChatSlowModeOn -= this.OnAppSlowModeOn;
            TwitchPlugin.Proxy.AppEvtChatSlowModeOff -= this.OnAppSlowModeOff;
            return true;
        }

        private void OnAppConnected(Object sender, EventArgs e)
        { }

        private void OnAppDisconnected(Object sender, EventArgs e)
        { }

        private void SetStateForItem(Int32 stateIndex, TimeSpanEventArg e)
        {
            try
            {
                //Reconstructing ActionParameter to set the state for specific action
                var d = new Dictionary<String, String>
                {
                    [SlowModeDurationControl] = e.Seconds.ToString()
                };

                this.SetCurrentState(new ActionEditorActionParameters(d), stateIndex);
            }
            catch (Exception ex)
            {
                TwitchPlugin.PluginLog.Error(ex, $"SloMo: Error setting state  {stateIndex} for {e.Seconds} time value");
            }
        }

        private void OnAppSlowModeOn(Object sender, TimeSpanEventArg e) => this.SetStateForItem(STATE_ON, e);
     
        private void OnAppSlowModeOff(Object sender, TimeSpanEventArg e) => this.SetStateForItem(STATE_OFF, e);

        private void OnActionEditorControlValueChanged(Object sender, ActionEditorControlValueChangedEventArgs e)
        {
            if (!e.ControlName.EqualsNoCase(SlowModeDurationControl))
            {
                //Wazzat? 
                return;
            }

            e.ActionEditorState.SetDisplayName($"Slow Mode for {e.ActionEditorState.GetControlValue(SlowModeDurationControl)} s");
        }

        private void OnActionEditorListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
            => ActionHelpers.FillListBox(e, SlowModeDurationControl, 
                () => Array.ForEach(ChatSlowModeCommand.SlowModeTimeSpans, 
                    (item) => e.AddItem(item.ToString(), $"{item} sec slow mode", $"Enables Slow mode for {item} sec")));
        
        protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 stateIndex, Int32 imageWidth, Int32 imageHeight)
        {
            var isOn = stateIndex == 1; //TwitchPlugin.Proxy.IsSlowMode
            var iconFileName = isOn ? this.ImgOn : this.ImgOff;

            var iconText = actionParameters.TryGetString(SlowModeDurationControl, out var modeDuration) 
                    ? $"{modeDuration} s" 
                    : "N/A";

            return (this.Plugin as TwitchPlugin).GetPluginCommandImage(imageWidth, imageHeight, iconFileName, iconText, iconFileName == this.ImgOn);
        }

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            if (!actionParameters.TryGetString(SlowModeDurationControl, out var modeDuration) || String.IsNullOrEmpty(modeDuration))
            {
                TwitchPlugin.Trace($"SlowMode: Cannot parse action parameters ");
                return false;
            }

            //We only switch off what we previously switched on. 
            if (TwitchPlugin.Proxy.IsSlowMode && modeDuration == TwitchPlugin.Proxy.SlowMode.ToString())
            {
                TwitchPlugin.Proxy.AppSlowModeOff();
            }
            else
            {
                if (Int32.TryParse(modeDuration, out var seconds))
                {
                    TwitchPlugin.Trace($"SlowMode: Turning on with {seconds} s");
                    TwitchPlugin.Proxy.AppSlowModeOn(seconds);
                }
                else
                {
                    TwitchPlugin.Trace($"SlowMode: Cannot parse action {modeDuration}");
                }
            }
            return true;
        }

    }
}
