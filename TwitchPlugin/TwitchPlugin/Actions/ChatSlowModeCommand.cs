namespace Loupedeck.TwitchPlugin.Actions
{
    using System;
    using System.Collections.Generic;

    internal class ChatSlowModeCommand : MultistateActionEditorCommand
    {
        private static readonly UInt32[] SlowModeTimeSpans = new UInt32[] { 1, 3, 5, 10, 15, 30, 60, 120 };

        private static readonly Dictionary<String, String> _allSlowCommandsActionParameters = new Dictionary<String, String>()
        {
            [SlowModeDurationControl] = "1",
            [SlowModeDurationControl] = "3",
            [SlowModeDurationControl] = "5",
            [SlowModeDurationControl] = "10",
            [SlowModeDurationControl] = "15",
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
        //=> this.IsEnabled = true;

        private void OnAppDisconnected(Object sender, EventArgs e)
        { }
        //=> this.IsEnabled = false;

        private void SetStateForItem(Int32 stateIndex, TimeSpanEventArg e)
        {
            TwitchPlugin.PluginLog.Info($"SloMo: Setting state {stateIndex} for TimeSpan item {e.Seconds}");

            try
            {
                //Reconstructing ActionParameter to set the state for specific action
                var d = new Dictionary<String, String>
                {
                    [SlowModeDurationControl] = e.Seconds.ToString()
                };

                this.SetCurrentState(new ActionEditorActionParameters(d), stateIndex);

                this.ActionImageChanged();
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
        {
            if (!e.ControlName.EqualsNoCase(SlowModeDurationControl))
            {
                //Wazzat? 
                return;
            }

            for (var i = 0; i < ChatSlowModeCommand.SlowModeTimeSpans.Length; i++)
            {
                var ts = ChatSlowModeCommand.SlowModeTimeSpans[i].ToString();
                e.AddItem(ts, $"{ts} sec slow mode", $"Enables Slow mode for {ChatSlowModeCommand.SlowModeTimeSpans[i]} sec");
            }
        }

        protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 stateIndex, Int32 imageWidth, Int32 imageHeight)
        {
            //We just return the same image with different text
            var isOn = TwitchPlugin.Proxy.IsSlowMode; // stateIndex == 1;
            var iconFileName = this.ImgOff;

            var iconText = "N/A";

            if (actionParameters.TryGetString(SlowModeDurationControl, out var modeDuration))
            {
                //When in slow mode, if the 'time' is not ours (this button's) we put label as "(duration) s". If it is ours, we put it as "duration s"
                iconText = $"{modeDuration} s";

                //Which one was on?  
                if (isOn && modeDuration == TwitchPlugin.Proxy.SlowMode.ToString())
                {
                    iconFileName = this.ImgOn;
                }

                TwitchPlugin.PluginLog.Info($"GetCommandImage: modeDuration{modeDuration} state={stateIndex} IsOn? {isOn} imgname = {iconFileName}");
            }

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


        /****/
    }
}
