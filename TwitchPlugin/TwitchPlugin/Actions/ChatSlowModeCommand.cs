namespace Loupedeck.TwitchPlugin.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Remoting.Messaging;

    internal class ChatSlowModeCommand : PluginMultistateDynamicCommand
    {
        private static readonly UInt32[] SlowModeTimeSpans = new UInt32[]{ 1, 3, 5, 10, 15, 30, 60, 120 };

        private readonly String ImgSlowModeOn = "TwitchSlowChat.png";
        private readonly String ImgSlowModeOff = "TwitchSlowChatToggle.png";

        //public static readonly Dictionary<, String> SlowModeNamesMap =
        //   new[] { "1", "3", "5", "10", "15", "30", "60", "120" }.ToDictionary(item => item, item => $"{item} Sec");

        public ChatSlowModeCommand()
        {
            this.DisplayName = "Chat Slow mode";
            this.Description = "Turns Slow Mode for Twitch Chat on/off";
            this.GroupName = "Chat Slow Mode";
            this.Name = "ChatSlowMode";

            this.AddState("Off", "Slow mode off");
            this.AddState("On", "Slow mode on");

            for (var i = 0; i < ChatSlowModeCommand.SlowModeTimeSpans.Count(); i++)
            {
                var ts = ChatSlowModeCommand.SlowModeTimeSpans[i].ToString();
                var p = this.AddParameter(ts, $"{ts} sec slow mode", this.GroupName);
                p.Description = $"Enables Slow mode for {ChatSlowModeCommand.SlowModeTimeSpans[i]} sec";
                this.SetCurrentState(ts, 0);
            }
        }

        protected override Boolean OnLoad()
        {
            TwitchPlugin.Proxy.AppConnected += this.OnAppConnected;
            TwitchPlugin.Proxy.AppDisconnected += this.OnAppDisconnected;
            TwitchPlugin.Proxy.AppEvtChatSlowModeOn += this.OnAppSlowModeOn;
            TwitchPlugin.Proxy.AppEvtChatSlowModeOff += this.OnAppSlowModeOff;

            this.IsEnabled = false;
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

        private void OnAppConnected(Object sender, EventArgs e) => this.IsEnabled = true;

        private void OnAppDisconnected(Object sender, EventArgs e) => this.IsEnabled = false;

        private void OnAppSlowModeOn(Object sender, TimeSpanEventArg e)
        {
            //When SLOW mode is on, we set ALL parameters to ON!
            //Setting all parameters except for the one e.SlowMode to off
            foreach(var p in this.GetParameters())
            {   
                this.SetCurrentState(p.Name, /*(p.Name != e.SlowModeRange.ToString()) ? 0 :*/ 1);
            }
            this.ParametersChanged();
        }

        private void OnAppSlowModeOff(Object sender, EventArgs e)
        {
            foreach (var p in this.GetParameters())
            {
                this.SetCurrentState(p.Name, 0);
            }
            this.ParametersChanged();
        }


        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var iconText = $"{actionParameter} sec";
            return (this.Plugin as TwitchPlugin).GetPluginCommandImage(imageSize, TwitchPlugin.Proxy.IsSlowMode ? this.ImgSlowModeOn : this.ImgSlowModeOff,iconText);
        }
            

        protected override void RunCommand(String actionParameter)
        {
            //NB: Slow mode act differently WRT parameters: When Slow mode is on,
            //all parameters are On, pressing on it would switch it off
            if(TwitchPlugin.Proxy.IsSlowMode)
            {
                TwitchPlugin.Proxy.AppToggleSlowMode(0);
            }
            else //We switch on according to what was pressed
            {
                if( Int32.TryParse(actionParameter, out var modeDuration) )
                {
                    TwitchPlugin.Proxy.AppToggleSlowMode(modeDuration);
                }
                else
                {
                    TwitchPlugin.Trace($"SlowMode: Cannot parse action param {actionParameter}");
                }
            }


        }

    }
}
