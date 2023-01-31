using System.Collections.Generic;
using System;

namespace Loupedeck.TwitchPlugin.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
   
    internal class ChatFollowersCommand : PluginMultistateDynamicCommand
    {
        private static readonly KeyValuePair<System.TimeSpan, String>[] FollowModeTimeSpans =
                new KeyValuePair<System.TimeSpan, String>[] {
                        new KeyValuePair<System.TimeSpan, String>(new System.TimeSpan( 0, 0, 0,0), "0 (All)"),
                        new KeyValuePair<System.TimeSpan, String>(new System.TimeSpan( 0, 0,10,0), "10 Minutes"),
                        new KeyValuePair<System.TimeSpan, String>(new System.TimeSpan( 0, 0,30,0), "30 Minutes"),
                        new KeyValuePair<System.TimeSpan, String>(new System.TimeSpan( 0, 1, 0,0), "1 Hour"),
                        new KeyValuePair<System.TimeSpan, String>(new System.TimeSpan( 1, 0, 0,0), "1 Day"),
                        new KeyValuePair<System.TimeSpan, String>(new System.TimeSpan( 7, 0, 0,0), "1 Week"),
                        new KeyValuePair<System.TimeSpan, String>(new System.TimeSpan(30, 0, 0,0), "1 Month"),
                        new KeyValuePair<System.TimeSpan, String>(new System.TimeSpan(90, 0, 0,0), "3 Months")
                };

        private readonly String ImgOn = "TwitchFollowerChat.png";
        private readonly String ImgOff = "TwitchFollowerChatToggle.png";

        public ChatFollowersCommand()
        {
            this.DisplayName = "Chat Followers-Only";
            this.Description = "Turns Followers-Only mode for Twitch Chat on/off";
            this.GroupName = "Chat Followers-Only";
            this.Name = "ChatFollowers";

            this.AddState("Off", "Followers-only chat off");
            this.AddState("On", "Followers-only chat on");

            //var n = 0;
            //foreach(var item in ChatFollowersCommand.FollowModeTimeSpans)
            for(var i=0;i< ChatFollowersCommand.FollowModeTimeSpans.Count();i++)
            {
                var item = ChatFollowersCommand.FollowModeTimeSpans[i];
                var key = item.Key.TotalSeconds.ToString();

                var p = this.AddParameter( key, key!="0" ?  $"Minimum {item.Value} followers-only chat" : "Followers-only chat (all followers)", this.GroupName); 
                p.Description = key != "0" ? $"Enables Followers-only mode for the chat for users who followed for at least {item.Value}" : $"Enables Followers-only mode for the chat for all followers";
                this.SetCurrentState(key, 0);
            }
        }

        protected override Boolean OnLoad()
        {
            TwitchPlugin.Proxy.AppConnected += this.OnAppConnected;
            TwitchPlugin.Proxy.AppDisconnected += this.OnAppDisconnected;
            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOn += this.OnAppFollowersOn;
            TwitchPlugin.Proxy.AppEvtChatFollowersOnlyOff += this.OnAppFollowersOff;

            this.IsEnabled = false;
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

        private void OnAppConnected(Object sender, EventArgs e) => this.IsEnabled = true;

        private void OnAppDisconnected(Object sender, EventArgs e) => this.IsEnabled = false;

        private void OnAppFollowersOn(Object sender, TimeSpanEventArg e)
        {
            //When SLOW mode is on, we set ALL parameters to ON!
            //Setting all parameters except for the one e.SlowMode to off
            foreach (var p in this.GetParameters())
            {
                this.SetCurrentState(p.Name, /*(p.Name != e.SlowModeRange.ToString()) ? 0 :*/ 1);
            }
            this.ParametersChanged();
        }

        private void OnAppFollowersOff(Object sender, EventArgs e)
        {
            foreach (var p in this.GetParameters())
            {
                this.SetCurrentState(p.Name, 0);
            }
            this.ParametersChanged();
        }


        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            String itemText = null;

            if( UInt32.TryParse(actionParameter, out var val))
            {
                var searchTimeSpan = TimeSpan.FromSeconds(val);
                var result = Array.Find(ChatFollowersCommand.FollowModeTimeSpans, x => x.Key == searchTimeSpan);
                if (result.Key != default)
                {
                    itemText = result.Value;
                }
            }
            return (this.Plugin as TwitchPlugin).GetPluginCommandImage(imageSize, TwitchPlugin.Proxy.IsFollowersOnly ? this.ImgOn : this.ImgOff, itemText);
        }
        


        protected override void RunCommand(String actionParameter)
        {
            //NB: Follow mode act differently WRT parameters: When Slow mode is on,
            //all parameters are On, pressing on it would switch it off
            if (TwitchPlugin.Proxy.IsFollowersOnly)
            {
                TwitchPlugin.Proxy.AppToggleFollowersOnly(0);
            }
            else //We switch on according to what was pressed
            {
                if (Int32.TryParse(actionParameter, out var modeDuration))
                {
                    TwitchPlugin.Proxy.AppToggleFollowersOnly(modeDuration);
                }
                else
                {
                    TwitchPlugin.Trace($"FollowierMode : Cannot parse action param {actionParameter}");
                }
            }


        }

    }
}
        