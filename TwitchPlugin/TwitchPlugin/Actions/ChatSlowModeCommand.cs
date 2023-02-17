﻿namespace Loupedeck.TwitchPlugin.Actions
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


        private readonly String ImgSlowModeOn  = "TwitchSlowChat.png";
        private readonly String ImgSlowModeOff = "TwitchSlowChatToggle.png";

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

        private void SetStateForAll(Int32 stateIndex)
        {
            foreach(var item in _allSlowCommandsActionParameters)
            {
                var d = new Dictionary<String, String>
                {
                    [item.Key] = item.Value
                };
                var p = new ActionEditorActionParameters(d);
                this.SetCurrentState(p, stateIndex);
                this.ActionImageChanged();
            }
        }

        private void OnAppSlowModeOn(Object sender, TimeSpanEventArg e)
        {
            TwitchPlugin.PluginLog.Info("OnAppSlowModeOn");

            this.SetStateForAll(STATE_ON);
            this.ActionImageChanged();
        }

        private void OnAppSlowModeOff(Object sender, EventArgs e)
        {
            TwitchPlugin.PluginLog.Info("OnAppSlowModeOff");
            this.SetStateForAll(STATE_OFF);
            this.ActionImageChanged();
        }

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
                //this.Plugin.Log.Info($"AE: Adding ('{v.Name}','{v.DisplayName}','{v.Description}')");
            }
            
        }


        protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 stateIndex, Int32 imageWidth, Int32 imageHeight)
        {
            //We just return the same image with different text
            var iconText = "N/A s";

            if( actionParameters.TryGetString(SlowModeDurationControl, out var modeDuration) )
            {
                //When in slow mode, if the 'time' is not ours (this button's) we put label as "(duration) s". If it is ours, we put it as "duration s"
                iconText = !TwitchPlugin.Proxy.IsSlowMode || modeDuration == TwitchPlugin.Proxy.SlowMode.ToString()
                          ? $"{modeDuration} s" : $"({modeDuration}) s";
            }

            // FIXME FIXME: It should be isSlowmode = stateIndex == 1;  BUT IT DOES NOT WORK NOW!
            var isSlowmode = TwitchPlugin.Proxy.IsSlowMode; // stateIndex == 1;
            var iconFileName = /*TwitchPlugin.Proxy.IsSlowMode*/ isSlowmode ? this.ImgSlowModeOn : this.ImgSlowModeOff;
            return  (this.Plugin as TwitchPlugin).GetPluginCommandImage(imageWidth == 90 ? PluginImageSize.Width90 : PluginImageSize.Width60, iconFileName, iconText, isSlowmode);
        }

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            //if we're in slow mode, we just swithing it off from any button. 
            if( TwitchPlugin.Proxy.IsSlowMode )
            {
                TwitchPlugin.Proxy.AppToggleSlowMode(0);
                return true;
            }

            if (!actionParameters.TryGetString(SlowModeDurationControl, out var slowmodeDuration))
            {
                TwitchPlugin.Trace($"SlowMode: Cannot get action param {actionParameters}");
            }

            if (Int32.TryParse(slowmodeDuration, out var modeDuration))
            {
                    TwitchPlugin.Proxy.AppToggleSlowMode(modeDuration);
            }
            else
            {
                TwitchPlugin.Trace($"SlowMode: Cannot parse action param {slowmodeDuration}");
            }
             
            return true;
        }

    }
}
