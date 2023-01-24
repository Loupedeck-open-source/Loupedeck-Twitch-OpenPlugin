namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Threading.Tasks;
    using TwitchLib.Api.Core.Exceptions;
    using TwitchLib.Client.Events;
    using System.Linq;

    public partial class TwitchWrapper : IDisposable
    {
        public EventHandler ChannelStatusChanged { get; set; }
        public Boolean IsSubOnly => this._channelState?.SubOnly == true;
        public Boolean IsEmoteOnly => this._channelState?.EmoteOnly == true;
        public Int32 SlowMode => this._channelState?.SlowMode ?? 0;
        public TimeSpan FollowersOnly => this._channelState?.FollowersOnly ?? TimeSpan.Zero;

        public Boolean IsSlowMode => this.SlowMode != 0;
        public Boolean IsFollowersOnly => this.FollowersOnly != TimeSpan.Zero;

        public event EventHandler<EventArgs> AppEvtChatEmotesOnlyOn;
        public event EventHandler<EventArgs> AppEvtChatEmotesOnlyOff;

        public event EventHandler<EventArgs> AppEvtChatSubscribersOnlyOn;
        public event EventHandler<EventArgs> AppEvtChatSubscribersOnlyOff;

        public event EventHandler<EventArgs> AppEvtChatFollowersOnlyOn;
        public event EventHandler<EventArgs> AppEvtChatFollowersOnlyOff;

        public event EventHandler<EventArgs> AppEvtChatSlowModeOn;
        public event EventHandler<EventArgs> AppEvtChatSlowModeOff;


        public void AppToggleEmotesOnly()
        {
            this.SendMessage(this.IsEmoteOnly ? ".emoteonlyoff" : ".emoteonly");
        }
        public void AppToggleSubscribersOnly()
        {
            this.SendMessage(this.IsSubOnly ? ".subscribersoff" : ".subscribers");
        }
        public void AppToggleFollowersOnly()
        {
            this.SendMessage(this.IsFollowersOnly ? ".followersoff" : ".followerson 10m");
        }
        public void AppToggleSlowMode()
        {
            this.SendMessage(TwitchPlugin.Proxy.IsSlowMode ? ".slowoff" : ".slow 30");
        }


        public void CreateMarkerCommand()
        {
            try
            {
                var request = new  TwitchLib.Api.Helix.Models.Streams.CreateStreamMarker.CreateStreamMarkerRequest();
                request.UserId = this._userInfo.Id;
                request.Description = "Loupedeck Marker";
               
                var result = this.twitchApi.Helix.Streams.CreateStreamMarkerAsync(request).Result;
                //Result.Data should actually result.Data
                var market_str = "";
                foreach(var d in result.Data)
                {
                    market_str += d.ToString() + ", ";
                }
                TwitchPlugin.PluginLog.Info($"Set Marker to the stream, returned data {market_str}");

                
            }
            catch (TokenExpiredException)
            {
                this.AccessTokenExpired?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Warning(e, "TwitchPlugin.CreateMarkerCommandAsync error: " + e.Message);
            }
        }

        public async Task CreateClipCommandAsync()
        {
            try
            {
                var clip = (await this.twitchApi.Helix.Clips.CreateClipAsync(this._userInfo.Id)).CreatedClips.First();
                this.SendMessage(clip.EditUrl);
            }
            catch (TokenExpiredException)
            {
                this.AccessTokenExpired?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Warning(e, "TwitchPlugin.CreateClipCommandAsync error: " + e.Message);
            }
        }

        private void OnChannelStateChanged(Object sender, OnChannelStateChangedArgs e)
        {
            if (e.Channel != this._userInfo.Login)
            {
                return;
            }

            if (this._twitchClient.JoinedChannels.All(c => !c.Channel.Equals(this._userInfo.Login)))
            {
                this._twitchClient.JoinChannel(this._userInfo.Login);
            }

            var prev_emote = this.IsEmoteOnly;
            var prev_sub = this.IsSubOnly;
            var prev_follow = this.FollowersOnly;
            var prev_slow = this.SlowMode;

            this._channelState = e.ChannelState;

            if (this.IsEmoteOnly != prev_emote)
            {
                if (this.IsEmoteOnly)
                {
                    this.AppEvtChatEmotesOnlyOn.Invoke(this, e);
                }
                else
                {
                    this.AppEvtChatEmotesOnlyOff.Invoke(this, e);
                }
            }

            if (this.IsSubOnly != prev_sub)
            {
                if (this.IsSubOnly)
                {
                    this.AppEvtChatSubscribersOnlyOn.Invoke(this, e);
                }
                else
                {
                    this.AppEvtChatSubscribersOnlyOff.Invoke(this, e);
                }
            }
            /*
             *  This is not yet needed
                        if (this.FollowersOnly != prev_follow)
                        {
                            if (this.IsFollowersOnly)
                            {
                                this.AppEvtChatFollowersOnlyOn.Invoke(this, e);
                            }
                            else
                            {
                                this.AppEvtChatFollowersOnlyOff.Invoke(this, e);
                            }
                        }

                        if (this.SlowMode != prev_slow)
                        {
                            if (this.IsSlowMode)
                            {
                                this.AppEvtChatSlowModeOn.Invoke(this, e);
                            }
                            else
                            {
                                this.AppEvtChatSlowModeOff.Invoke(this, e);
                            }
                        }
            */
            this.ChannelStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnJoinedChannel(Object sender, OnJoinedChannelArgs e)
        {
            TwitchPlugin.PluginLog.Info($"Joined channel: {e.Channel}");
        }

        private void JoinChannel(String channel, Action callback = null)
        {
            void ChannelJoined(Object sender, OnChannelStateChangedArgs e)
            {
                if (!e.Channel.Equals(channel))
                {
                    return;
                }

                this._twitchClient.OnChannelStateChanged -= ChannelJoined;
                callback?.Invoke();
            }

            this._twitchClient.OnChannelStateChanged += ChannelJoined;
            this._twitchClient.JoinChannel(channel);
        }

    }

}
