namespace TwitchLib.Client.Models
{
    using System;
    using Internal;

    /// <summary>Class representing a channel state as received from Twitch chat.</summary>
    public class ChannelState
    {
        /// <summary>Property representing the current broadcaster language.</summary>
        public String BroadcasterLanguage { get; private set; }
        /// <summary>Property representing the current channel.</summary>
        public String Channel { get; private set; }
        /// <summary>Property representing whether EmoteOnly mode is being applied to chat or not. WILL BE NULL IF VALUE NOT PRESENT.</summary>
        public Boolean? EmoteOnly { get; private set; }
        /// <summary>Property representing how long needed to be following to talk </summary>
        public TimeSpan FollowersOnly { get; private set; }
        /// <summary>Property representing mercury value. Not sure what it's for.</summary>
        public Boolean Mercury { get; private set; }
        /// <summary>Property representing whether R9K is being applied to chat or not. WILL BE NULL IF VALUE NOT PRESENT.</summary>
        public Boolean? R9K { get; private set; }
        /// <summary>Property representing whether Rituals is enabled or not. WILL BE NULL IF VALUE NOT PRESENT.</summary>
        public Boolean? Rituals { get; private set; }
        /// <summary>Twitch assignedc room id</summary>
        public String RoomId { get; private set; }
        /// <summary>Property representing whether Slow mode is being applied to chat or not. WILL BE NULL IF VALUE NOT PRESENT.</summary>
        public Int32? SlowMode { get; private set; }
        /// <summary>Property representing whether Sub Mode is being applied to chat or not. WILL BE NULL IF VALUE NOT PRESENT.</summary>
        public Boolean? SubOnly { get; private set; }

        public ChannelState()
        { }

        public ChannelState(Boolean r9k, Boolean rituals, Boolean subonly, Int32 slowMode, Boolean emoteOnly, String broadcasterLanguage, String channel, TimeSpan followersOnly, Boolean mercury, String roomId)
        {
            this.R9K = r9k;
            this.Rituals = rituals;
            this.SubOnly = subonly;
            this.SlowMode = slowMode;
            this.EmoteOnly = emoteOnly;
            this.BroadcasterLanguage = broadcasterLanguage;
            this.Channel = channel;
            this.FollowersOnly = followersOnly;
            this.Mercury = mercury;
            this.RoomId = roomId;
        }

        public void Update(IrcMessage ircMessage)
        {
            //@broadcaster-lang=;emote-only=0;r9k=0;slow=0;subs-only=1 :tmi.twitch.tv ROOMSTATE #burkeblack
            foreach (var tag in ircMessage.Tags.Keys)
            {
                var tagValue = ircMessage.Tags[tag];

                switch (tag)
                {
                    case Tags.BroadcasterLang:
                        this.BroadcasterLanguage = tagValue;
                        break;
                    case Tags.EmoteOnly:
                        this.EmoteOnly = Common.Helpers.ConvertToBool(tagValue);
                        break;
                    case Tags.R9K:
                        this.R9K = Common.Helpers.ConvertToBool(tagValue);
                        break;
                    case Tags.Rituals:
                        this.Rituals = Common.Helpers.ConvertToBool(tagValue);
                        break;
                    case Tags.Slow:
                        var success = Int32.TryParse(tagValue, out var slowDuration);
                        this.SlowMode = success ? slowDuration : (Int32?)null;
                        break;
                    case Tags.SubsOnly:
                        this.SubOnly = Common.Helpers.ConvertToBool(tagValue);
                        break;
                    case Tags.FollowersOnly:
                        var minutes = Int32.Parse(tagValue);
                        this.FollowersOnly = TimeSpan.FromMinutes(minutes == -1 ? 0 : minutes);
                        break;
                    case Tags.RoomId:
                        this.RoomId = tagValue;
                        break;
                    case Tags.Mercury:
                        this.Mercury = Common.Helpers.ConvertToBool(tagValue);
                        break;
                    default:
                        Console.WriteLine("[TwitchLib][ChannelState] Unaccounted for: " + tag);
                        break;
                }
            }

            this.Channel = ircMessage.Channel;
        }

    }
}
