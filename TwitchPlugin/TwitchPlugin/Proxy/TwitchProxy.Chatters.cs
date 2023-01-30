
namespace Loupedeck.TwitchPlugin
{ 
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TwitchLib.Api.Core.Models.Undocumented.Chatters;
    using TwitchLib.Client.Events;

    public partial class TwitchPlugin : IDisposable
    {
        public EventHandler<HashSet<String>> ChattersChanged { get; set; }

        public HashSet<String> Chatters { get; } = new HashSet<String>();

        private void OnUserLeft(Object sender, OnUserLeftArgs e)
        {
            lock (this._viewersLock)
            {
                if (e.Username != this._userInfo.Login)
                {
                    this.Chatters.Remove(e.Username);
                }
            }

            this.ChattersChanged?.Invoke(this, this.Chatters);
        }

        private void OnUserJoined(Object sender, OnUserJoinedArgs e)
        {
            lock (this._viewersLock)
            {
                if (e.Username != this._userInfo.Login)
                {
                    this.Chatters.Add(e.Username);
                }
            }
            this.ChattersChanged?.Invoke(this, this.Chatters);
        }

        // This function uses an undocumented endpoint, meaning it can break at any time.
        // https://discuss.dev.twitch.tv/t/getting-current-viewers-of-a-twitch-stream
        private async Task FetchChattersAsync()
        {
            this.Chatters.Clear();
            this.ChattersChanged?.Invoke(this, this.Chatters);

            List<ChatterFormatted> chatters = null;

            var fetched = false;
            var tryCount = 0;
            while (!fetched && tryCount < 10)
            {
                try
                {
                    tryCount++;
                    chatters = await this._twitchApi.Undocumented.GetChattersAsync(this._userInfo.Login);
                    fetched = true;
                }
                catch (Exception e)
                {
                    TwitchPlugin.PluginLog.Warning(e, "TwitchPlugin.FetchViewersAsync error: " + e.Message);
                }
            }

            lock (this._viewersLock)
            {
                foreach (var chatter in chatters)
                {
                    if (chatter.Username != this._userInfo.Login)
                    {
                        this.Chatters.Add(chatter.Username);
                    }
                }
            }

            this.ChattersChanged?.Invoke(this, this.Chatters);
        }

    }

}
