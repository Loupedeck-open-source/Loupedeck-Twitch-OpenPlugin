namespace Loupedeck.TwitchPlugin.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
   
    internal class ChatFollowersCommand : MultistateActionEditorCommand
    {
        private class FollowModeDescriptor
        {
            public Int32 durationSeconds;
            public String longName;
            public String shortName;
            public FollowModeDescriptor(System.TimeSpan duration, String name, String sname)
            {
                this.durationSeconds = (Int32)duration.TotalSeconds;
                this.longName = name;
                this.shortName = sname;
            }
        }

        private static readonly FollowModeDescriptor[] FollowModeTimeSpans =
        {
            new FollowModeDescriptor(new System.TimeSpan( 0, 0, 0,0), "0 Minutes (All)","all"),
            new FollowModeDescriptor(new System.TimeSpan( 0, 0,10,0), "10 Minutes","10m"),
            new FollowModeDescriptor(new System.TimeSpan( 0, 0,30,0), "30 Minutes","30m"),
            new FollowModeDescriptor(new System.TimeSpan( 0, 1, 0,0), "1 Hour","1h"),
            new FollowModeDescriptor(new System.TimeSpan( 1, 0, 0,0), "1 Day","1d"),
            new FollowModeDescriptor(new System.TimeSpan( 7, 0, 0,0), "1 Week","1w"),
            new FollowModeDescriptor(new System.TimeSpan(30, 0, 0,0), "1 Month","1mo"),
            new FollowModeDescriptor(new System.TimeSpan(90, 0, 0,0), "3 Months","3mo")
        };
        
        private readonly String ImgOn = "TwitchFollowerChat.png";
        private readonly String ImgOff = "TwitchFollowerChatToggle.png";

        private const Int32 STATE_OFF = 0;
        private const Int32 STATE_ON = 1;


        private const String FollowModeDurationControl = "followModeDuration";
        private const String OnStateName = "ON";
        private const String OffStateName = "OFF";

        public ChatFollowersCommand()
        {
            this.DisplayName = "Chat Followers-Only";
            this.Description = "Turns Followers-Only mode for Twitch Chat on/off";
            this.GroupName = "Chat Followers-Only";

            this.ActionEditor.AddControl(
                           new ActionEditorListbox(name: FollowModeDurationControl, labelText: "Followers for:"));

            this.ActionEditor.ListboxItemsRequested += this.OnActionEditorListboxItemsRequested;
            this.ActionEditor.ControlValueChanged += this.OnActionEditorControlValueChanged;

            this.AddState(OffStateName, "Followers-only chat off");
            this.AddState(OnStateName, "Followers-only chat on");
        }

        protected override Boolean OnLoad()
        {
            TwitchPlugin.Proxy.AppConnected += this.OnAppConnected;
            TwitchPlugin.Proxy.AppDisconnected += this.OnAppDisconnected;
            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOn += this.OnAppFollowersOn;
            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOff += this.OnAppFollowersOff;
        
            return true;
        }

        protected override Boolean OnUnload()
        {
            TwitchPlugin.Proxy.AppConnected -= this.OnAppConnected;
            TwitchPlugin.Proxy.AppDisconnected -= this.OnAppDisconnected;
            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOn -= this.OnAppFollowersOn;
            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOff -= this.OnAppFollowersOff;

            return true;
        }

        private void OnAppConnected(Object sender, EventArgs e)
        {

        }

        private void OnAppDisconnected(Object sender, EventArgs e)
        {

        }

        private UInt32 GetSeconds(TimeSpan t) =>(UInt32)t.TotalSeconds;

        private void SetStateForItem(Int32 stateIndex, TimeSpanEventArg e)
        {
            try
            {
                //Reconstructing ActionParameter to set the state for specific action
                var d = new Dictionary<String, String>
                {
                    [FollowModeDurationControl] = e.Seconds.ToString()
                };
                this.SetCurrentState(new ActionEditorActionParameters(d), stateIndex);
            }
            catch (Exception ex)
            {
                TwitchPlugin.PluginLog.Error(ex, $"Followers: Error setting state  {stateIndex} for {e.Seconds} time value");
            }
        }

        private void OnAppFollowersOn(Object sender, TimeSpanEventArg e) => this.SetStateForItem(STATE_ON, e);

        private void OnAppFollowersOff(Object sender, TimeSpanEventArg e) => this.SetStateForItem(STATE_OFF, e);

        private void OnActionEditorControlValueChanged(Object sender, ActionEditorControlValueChangedEventArgs e)
        {
            if (!e.ControlName.EqualsNoCase(FollowModeDurationControl))
            {
                //Wazzat? 
                return;
            }

            var item = Array.Find(ChatFollowersCommand.FollowModeTimeSpans, x => x.durationSeconds.ToString() == e.ActionEditorState.GetControlValue(FollowModeDurationControl));
            if (!Object.Equals(item, default(FollowModeDescriptor)))
            {
                e.ActionEditorState.SetDisplayName($"Followers-Only {item.longName}");
            }
        }

        private void OnActionEditorListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
            => ActionHelpers.FillListBox(e, FollowModeDurationControl, 
                () => Array.ForEach(ChatFollowersCommand.FollowModeTimeSpans, 
                    (item)  => e.AddItem(name: item.durationSeconds.ToString(), displayName:  $"{item.longName}", description: $"Followers can chat if they followed you at least {item.longName}")));
        
        protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 stateIndex, Int32 imageWidth, Int32 imageHeight)
        {
            var isOn = stateIndex == 1; //TwitchPlugin.Proxy.IsFollowersOnly
            var iconFileName = isOn ? this.ImgOn : this.ImgOff;

            var iconText = "N/A";

            if (actionParameters.TryGetString(FollowModeDurationControl, out var modeDuration))
            {
                //To get short name
                var item = Array.Find(ChatFollowersCommand.FollowModeTimeSpans, x => x.durationSeconds.ToString() == modeDuration);
                if (!Object.Equals(item, default(FollowModeDescriptor)))
                {
                    iconText = $"{item.shortName}";
                }
            }

            return (this.Plugin as TwitchPlugin).GetPluginCommandImage(imageWidth, imageHeight, iconFileName, iconText, iconFileName == this.ImgOn);
        }

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            if (!actionParameters.TryGetString(FollowModeDurationControl, out var modeDuration) || String.IsNullOrEmpty(modeDuration))
            {
                TwitchPlugin.Trace($"FollowerMode : Cannot parse action parameters ");
                return false;
            }

            //We only switch off what we previously switched on. 
            if (TwitchPlugin.Proxy.IsFollowersOnly && modeDuration == this.GetSeconds(TwitchPlugin.Proxy.FollowersOnly).ToString())
            {
                TwitchPlugin.Proxy.AppFollowersOnlyOff();
            }
            else /* if(!TwitchPlugin.Proxy.IsFollowersOnly ) */ //We switch on according to what was pressed, even if already in the mode
            {
                if (Int32.TryParse(modeDuration, out var seconds))
                {
                    TwitchPlugin.Trace($"FollowerMode : Turning on with {seconds} s");
                    TwitchPlugin.Proxy.AppFollowersOnlyOn(seconds);
                }
                else
                {
                    TwitchPlugin.Trace($"FollowerMode : Cannot parse action {modeDuration}");
                }
            }
            return true;
        }
    }
}       