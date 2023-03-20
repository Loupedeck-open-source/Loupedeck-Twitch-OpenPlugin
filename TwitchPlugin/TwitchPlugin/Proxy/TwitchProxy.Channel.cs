namespace Loupedeck.TwitchPlugin
{
    using System;
    using System.Threading.Tasks;
    using TwitchLib.Api.Core.Exceptions;
    using TwitchLib.Client.Events;
    using System.Linq;
    using TwitchLib.Client.Models;
    using TwitchLib.Api.Helix.Models.Chat.ChatSettings;

    public partial class TwitchProxy : IDisposable
    {

        public event EventHandler<EventArgs> AppEvtChatEmotesOnlyOn;
        public event EventHandler<EventArgs> AppEvtChatEmotesOnlyOff;

        public event EventHandler<EventArgs> AppEvtChatSubscribersOnlyOn;
        public event EventHandler<EventArgs> AppEvtChatSubscribersOnlyOff;

        public event EventHandler<TimeSpanEventArg> AppEvtChatSlowModeOn;
        public event EventHandler<TimeSpanEventArg> AppEvtChatSlowModeOff;

        public event EventHandler<TimeSpanEventArg> AppEvtChatFollowersOnlyOn;
        public event EventHandler<TimeSpanEventArg> AppEvtChatFollowersOnlyOff;

        public JoinedChannel GetOwnChannel() => this._twitchClient.GetJoinedChannel(this._userInfo.Login);

        private void UpdateOwnChannelSettings(ChatSettings newSettings) 
            => this.EnsureOnOwnChannel(() => 
            {
                var _ = this.twitchApi.Helix.Chat.UpdateChatSettingsAsync(this._userInfo.Id, this._userInfo.Id, newSettings).Result;
                
            });

        /* Emotes-only */
        public Boolean IsEmoteOnly { get; private set; } = false;
        private void AppSetEmotesOnly(Boolean status) => this.UpdateOwnChannelSettings(new ChatSettings() { EmoteMode = status });
        public void AppEmotesOnlyOn() => this.AppSetEmotesOnly(true);
        public void AppEmotesOnlyOff() => this.AppSetEmotesOnly(false);
        public void AppToggleEmotesOnly() => this.AppSetEmotesOnly(!this.IsEmoteOnly);

        /* Subscribers-only */
        public Boolean IsSubOnly { get; private set; } = false;
        private void AppSetSubscribersOnly(Boolean status) => this.UpdateOwnChannelSettings(new ChatSettings() { SubscriberMode = status });
        public void AppToggleSubscribersOnly() => this.AppSetSubscribersOnly(!this.IsSubOnly);
        public void AppSubscribersOnlyOn() => this.AppSetSubscribersOnly(true);
        public void AppSubscribersOnlyOff() => this.AppSetSubscribersOnly(false);

        /* Followers-only */
        public static TimeSpan FollowersModeOff = TimeSpan.MinValue;
        public TimeSpan FollowersOnly { get; private set; } = TwitchProxy.FollowersModeOff;
        public Boolean IsFollowersOnly => this.FollowersOnly != TwitchProxy.FollowersModeOff;
        /* IMPORTANT:  New API takes duration in minutes, while old one was using seconds. We translate it right here*/        
        private void AppSetFollowersOnly(Boolean status, Int32 duration = 0) => this.UpdateOwnChannelSettings(status ? new ChatSettings() { FollowerMode = true, FollowerModeDuration = (int) duration/60 }: new ChatSettings() { FollowerMode = false });
        public void AppFollowersOnlyOn(Int32 duration) => this.AppSetFollowersOnly(true, duration);
        public void AppFollowersOnlyOff() => this.AppSetFollowersOnly(false);

        public void AppToggleFollowersOnly(Int32 duration = 0) => this.AppSetFollowersOnly(!(this.IsFollowersOnly || duration == 0));

        /* Slow mode */
        public Int32 SlowMode { get; private set; } = 0;
        public Boolean IsSlowMode => this.SlowMode != 0;
        private void AppSetSlowMode(Boolean status, Int32 duration = 0) => this.UpdateOwnChannelSettings(status ? new ChatSettings() { SlowMode = true, SlowModeWaitTime = duration } : new ChatSettings() { SlowMode = false });

        public void AppToggleSlowMode(Int32 duration = 0) => this.AppSetSlowMode(!(this.IsSlowMode || duration == 0));

        public void AppSlowModeOn(Int32 duration) => this.AppSetSlowMode(true, duration);
        public void AppSlowModeOff() => this.AppSetSlowMode(false);

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
                this.OnTwitchAccessTokenExpired?.BeginInvoke(this, EventArgs.Empty);
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
                this.OnTwitchAccessTokenExpired?.BeginInvoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                TwitchPlugin.PluginLog.Warning(e, "TwitchPlugin.CreateClipCommandAsync error: " + e.Message);
            }
        }

        //Ensures that we have joined own channel and, if not, joins channel and optionally runs the callback
        private void EnsureOnOwnChannel(Action callback = null)
        {
            if( this._twitchClient == null || !this.IsConnected)
            {
                return;
            }

            if (this._twitchClient.JoinedChannels.All(c => !c.Channel.Equals(this._userInfo.Login)))
            {
               this.JoinChannel(this._userInfo.Login,callback);
            }
            else
            {
               if(!Helpers.TryExecuteAction(() => callback?.Invoke()))
               {
                    TwitchPlugin.PluginLog.Error("TwitchPlugin.EnsureOnOwnChannel: Callback error!");
               }
            }
        }
         
        private void UpdateChannelStatusFromTwitch(OnChannelStateChangedArgs a)
        {
            ChannelState state = a?.ChannelState;
            if (state?.SubOnly != null)
            {
                this.IsSubOnly = state.SubOnly == true;
            }
            if (state?.EmoteOnly != null)
            {
                this.IsEmoteOnly = state?.EmoteOnly == true;
            }
            if (state?.SlowMode != null)
            {
                this.SlowMode = state?.SlowMode ?? 0;
            }
            //var chatSettings = twitchApi.Chat.GetChatSettingsAsync("CHANNEL_NAME").Result;
            if (state?.FollowersOnly != null)
            {
                this.FollowersOnly = state?.FollowersOnly ?? TwitchProxy.FollowersModeOff;
            }
        }
       
        private void OnChannelStateChanged(Object sender, OnChannelStateChangedArgs e)
        {
            void ConditionallyFireEvent(Boolean changed, Boolean condition, EventHandler<EventArgs> changedToTrueEvent, EventHandler<EventArgs> changedToFalseEvent,EventArgs ChangedToTrueEventArg=null)
            {
                if (changed)
                {
                    if (condition)
                    {
                        changedToTrueEvent?.Invoke(this, ChangedToTrueEventArg ?? e);
                    }
                    else
                    {
                        changedToFalseEvent?.Invoke(this, ChangedToTrueEventArg ?? e);
                    }
                }
            }

            if (e.Channel != this._userInfo.Login)
            {
                return;
            }


            this.EnsureOnOwnChannel(()=>
            {
                var prev_emote = this.IsEmoteOnly;
                var prev_sub = this.IsSubOnly;
                var prev_follow = this.FollowersOnly;
                var prev_slow = this.SlowMode;

                this.UpdateChannelStatusFromTwitch(e);

                ConditionallyFireEvent(this.IsEmoteOnly != prev_emote, this.IsEmoteOnly, this.AppEvtChatEmotesOnlyOn, this.AppEvtChatEmotesOnlyOff);
                ConditionallyFireEvent(this.IsSubOnly != prev_sub, this.IsSubOnly, this.AppEvtChatSubscribersOnlyOn, this.AppEvtChatSubscribersOnlyOff);


                if (this.FollowersOnly != prev_follow)
                {
                    if (this.IsFollowersOnly)
                    {
                        var arg = new TimeSpanEventArg(this.FollowersOnly);
                        TwitchPlugin.PluginLog.Info($"Received FollowersOnly for {this.FollowersOnly.TotalSeconds}");
                        if (prev_follow != TwitchProxy.FollowersModeOff)
                        {
                            //Meaning, we are switching between different slow modes
                            this.AppEvtChatFollowersOnlyOff?.Invoke(this, new TimeSpanEventArg(prev_follow));
                        }

                        this.AppEvtChatFollowersOnlyOn?.Invoke(this, arg);
                    }
                    else
                    {
                        var arg = new TimeSpanEventArg((Int32)prev_follow.TotalSeconds);
                        TwitchPlugin.PluginLog.Info($"Received FollowersOnlyOff");
                        this.AppEvtChatFollowersOnlyOff?.Invoke(this, arg);
                    }
                }

                if (this.SlowMode != prev_slow)
                {
                    if (this.IsSlowMode)
                    {
                        TwitchPlugin.PluginLog.Info($"Received SlowModeOn for {this.SlowMode} s");
                        if (prev_slow != 0)
                        {
                            //Meaning, we are switching between different slow modes
                            this.AppEvtChatSlowModeOff?.Invoke(this, new TimeSpanEventArg(prev_slow));
                        }

                        this.AppEvtChatSlowModeOn?.Invoke(this, new TimeSpanEventArg(this.SlowMode));
                    }
                    else
                    {
                        var arg = new TimeSpanEventArg(prev_slow);
                        TwitchPlugin.PluginLog.Info($"Received SlowModeOff");
                        this.AppEvtChatSlowModeOff?.Invoke(this, arg);
                    }
                }

            });
        }

        private void OnJoinedChannel(Object sender, OnJoinedChannelArgs e) => TwitchPlugin.PluginLog.Info($"Joined channel: {e.Channel}");

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

            TwitchPlugin.PluginLog.Info($"Joining channel {channel}");
            this._twitchClient.JoinChannel(channel);
        }

        private void OnUnaccountedFor(Object e, OnUnaccountedForArgs args)
        {
            // Sometimes, the status of the channel we know and the status of the real
            // channel go out-of-sync, and then we start receiving this type of NOTICE messages:
            // "@msg-id=already_subs_on :tmi.twitch.tv NOTICE #laperie :This room is already in subscribers-only mode."
            // We will parse them here and make sure our internal status is up-to-date with the channel

            const String MsgIdPrefix = "@msg-id=";
            const String MsgAlready = "already_";

            var tag = ""; 
            if(!Helpers.TryExecuteSafe(() =>
            {
                var startIndex = args.RawIRC.IndexOf(MsgIdPrefix);
                startIndex += MsgIdPrefix.Length;
                var endIndex = -1;
                if (startIndex > -1)
                {
                    endIndex = args.RawIRC.IndexOf(" ", startIndex);
                    
                }
                if (endIndex > -1)
                {
                    tag = args.RawIRC.Substring(startIndex, endIndex - startIndex);

                    //if string starts with the "already_", we remove it
                    if(tag.StartsWith(MsgAlready))
                    {
                        tag = tag.Remove(0,MsgAlready.Length);
                    }
                }
            }
            ))
            {
                TwitchPlugin.PluginLog.Warning("OnUnaccountedFor: Exception!");
            }
            else if (tag!="")
            {
                TwitchPlugin.PluginLog.Info($"Retreived unaccounted tag: {tag}");

                //TODO : Figure out this ON/OFF state correspondence!
                switch (tag)
                {
                    case "emote_only_off":   
                        this.AppEvtChatEmotesOnlyOff?.Invoke(this, null); 
                        break;

                    case "emote_only_on":  
                        this.AppEvtChatEmotesOnlyOn?.Invoke(this, null); 
                        break;

                    case "subs_off": 
                        this.AppEvtChatSubscribersOnlyOff?.Invoke(this, null); 
                        break;

                    case "subs_on": 
                        this.AppEvtChatSubscribersOnlyOn?.Invoke(this, null); 
                        break;

                    case "slow_off":
                        if (this.IsSlowMode)
                        {
                            TwitchPlugin.PluginLog.Info($"Note: Switching off Slow mode ");
                            this.SlowMode = 0;
                            this.AppEvtChatSlowModeOff?.Invoke(this, null);
                        }
                        break;

                    case "slow_on":
                        if (!this.IsSlowMode)
                        {
                            var seconds = 1;
                            if(Helpers.TryExecuteFunc(() =>
                            {
                                //@msg-id=slow_on :tmi.twitch.tv NOTICE #beeeightwelve : This room is now in slow mode. You may send messages every 120 seconds.
                                var input = args.RawIRC;
                                const String start = "every ";
                                const String end = " second";
                                var startIndex = input.IndexOf(start) + start.Length;
                                var endIndex = input.IndexOf(end, startIndex);
                                return (endIndex > 0 && startIndex > start.Length) 
                                       ? Int32.Parse(input.Substring(startIndex, endIndex - startIndex)) 
                                       : 2;
                            }, out var retval))
                            {
                                seconds = retval;
                            }
                            else
                            {
                                TwitchPlugin.PluginLog.Warning($"Cannot retreive slow seconds number from string");
                            }

                            TwitchPlugin.PluginLog.Info($"Note: Switching on  Slow mode for {seconds} seconds");
                            this.SlowMode = seconds;
                            this.AppEvtChatSlowModeOn?.Invoke(this, null);
                        }
                        break;

                }

            }
        }
    }
}