namespace TwitchLib.Client.Manager
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Models;

    internal class JoinedChannelManager
    {
        private readonly ConcurrentDictionary<String, JoinedChannel> _joinedChannels;

        public JoinedChannelManager()
        {
            this._joinedChannels = new ConcurrentDictionary<String, JoinedChannel>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddJoinedChannel(JoinedChannel joinedChannel)
        {
            this._joinedChannels.TryAdd(joinedChannel.Channel, joinedChannel);
        }

        public JoinedChannel GetJoinedChannel(String channel)
        {
            this._joinedChannels.TryGetValue(channel, out var joinedChannel);
            return joinedChannel;
        }

        public IReadOnlyList<JoinedChannel> GetJoinedChannels()
        {
            return this._joinedChannels.Values.ToList().AsReadOnly();
        }

        public void RemoveJoinedChannel(String channel)
        {
            this._joinedChannels.TryRemove(channel, out _);
        }

        public void Clear()
        {
            this._joinedChannels.Clear();
        }
    }
}