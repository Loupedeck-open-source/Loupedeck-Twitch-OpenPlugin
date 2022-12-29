namespace TwitchLib.Communication.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Clients;
    using Events;
    using Interfaces;

    public class Throttlers
    {
        public readonly BlockingCollection<Tuple<DateTime, String>> SendQueue =
            new BlockingCollection<Tuple<DateTime, String>>();

        public readonly BlockingCollection<Tuple<DateTime, String>> WhisperQueue =
            new BlockingCollection<Tuple<DateTime, String>>();

        public Boolean Reconnecting { get; set; } = false;
        public Boolean ShouldDispose { get; set; } = false;
        public CancellationTokenSource TokenSource { get; set; }
        public Boolean ResetThrottlerRunning;
        public Boolean ResetWhisperThrottlerRunning;
        public Int32 SentCount = 0;
        public Int32 WhispersSent = 0;
        public Task ResetThrottler;
        public Task ResetWhisperThrottler;

        private readonly TimeSpan _throttlingPeriod;
        private readonly TimeSpan _whisperThrottlingPeriod;
        private readonly IClient _client;

        public Throttlers(IClient client, TimeSpan throttlingPeriod, TimeSpan whisperThrottlingPeriod)
        {
            this._throttlingPeriod = throttlingPeriod;
            this._whisperThrottlingPeriod = whisperThrottlingPeriod;
            this._client = client;
        }

        public void StartThrottlingWindowReset()
        {
            this.ResetThrottler = Task.Run(async () =>
            {
                this.ResetThrottlerRunning = true;
                while (!this.ShouldDispose && !this.Reconnecting)
                {
                    Interlocked.Exchange(ref this.SentCount, 0);
                    await Task.Delay(this._throttlingPeriod, this.TokenSource.Token);
                }

                this.ResetThrottlerRunning = false;
                return Task.CompletedTask;
            });
        }

        public void StartWhisperThrottlingWindowReset()
        {
            this.ResetWhisperThrottler = Task.Run(async () =>
            {
                this.ResetWhisperThrottlerRunning = true;
                while (!this.ShouldDispose && !this.Reconnecting)
                {
                    Interlocked.Exchange(ref this.WhispersSent, 0);
                    await Task.Delay(this._whisperThrottlingPeriod, this.TokenSource.Token);
                }

                this.ResetWhisperThrottlerRunning = false;
                return Task.CompletedTask;
            });
        }

        public void IncrementSentCount()
        {
            Interlocked.Increment(ref this.SentCount);
        }

        public void IncrementWhisperCount()
        {
            Interlocked.Increment(ref this.WhispersSent);
        }

        public Task StartSenderTask()
        {
            this.StartThrottlingWindowReset();
            
            return Task.Run(async () =>
            {
                try
                {
                    while (!this.ShouldDispose && !this.TokenSource.IsCancellationRequested)
                    {
                        await Task.Delay(this._client.Options.SendDelay);

                        if (this.SentCount == this._client.Options.MessagesAllowedInPeriod)
                        {
                            this._client.MessageThrottled(new OnMessageThrottledEventArgs
                            {
                                Message =
                                    "Message Throttle Occured. Too Many Messages within the period specified in WebsocketClientOptions.",
                                AllowedInPeriod = this._client.Options.MessagesAllowedInPeriod,
                                Period = this._client.Options.ThrottlingPeriod,
                                SentMessageCount = Interlocked.CompareExchange(ref this.SentCount, 0, 0)
                            });

                            continue;
                        }

                        if (!this._client.IsConnected || this.ShouldDispose) continue;

                        var msg = this.SendQueue.Take(this.TokenSource.Token);
                        if (msg.Item1.Add(this._client.Options.SendCacheItemTimeout) < DateTime.UtcNow) continue;

                        try
                        {
                            switch (this._client)
                            {
                                case WebSocketClient ws:
                                    await ws.SendAsync(Encoding.UTF8.GetBytes(msg.Item2));
                                    break;
                                case TcpClient tcp:
                                    await tcp.SendAsync(msg.Item2);
                                    break;
                            }

                            this.IncrementSentCount();
                        }
                        catch (Exception ex)
                        {
                            this._client.SendFailed(new OnSendFailedEventArgs {Data = msg.Item2, Exception = ex});
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this._client.SendFailed(new OnSendFailedEventArgs {Data = "", Exception = ex});
                    this._client.Error(new OnErrorEventArgs {Exception = ex});
                }
            });
        }

        public Task StartWhisperSenderTask()
        {
            this.StartWhisperThrottlingWindowReset();
            
            return Task.Run(async () =>
            {
                try
                {
                    while (!this.ShouldDispose && !this.TokenSource.IsCancellationRequested)
                    {
                        await Task.Delay(this._client.Options.SendDelay);

                        if (this.WhispersSent == this._client.Options.WhispersAllowedInPeriod)
                        {
                            this._client.WhisperThrottled(new OnWhisperThrottledEventArgs()
                            {
                                Message =
                                    "Whisper Throttle Occured. Too Many Whispers within the period specified in ClientOptions.",
                                AllowedInPeriod = this._client.Options.WhispersAllowedInPeriod,
                                Period = this._client.Options.WhisperThrottlingPeriod,
                                SentWhisperCount = Interlocked.CompareExchange(ref this.WhispersSent, 0, 0)
                            });

                            continue;
                        }

                        if (!this._client.IsConnected || this.ShouldDispose) continue;

                        var msg = this.WhisperQueue.Take(this.TokenSource.Token);
                        if (msg.Item1.Add(this._client.Options.SendCacheItemTimeout) < DateTime.UtcNow) continue;

                        try
                        {
                            switch (this._client)
                            {
                                case WebSocketClient ws:
                                    await ws.SendAsync(Encoding.UTF8.GetBytes(msg.Item2));
                                    break;
                                case TcpClient tcp:
                                    await tcp.SendAsync(msg.Item2);
                                    break;
                            }

                            this.IncrementSentCount();
                        }
                        catch (Exception ex)
                        {
                            this._client.SendFailed(new OnSendFailedEventArgs {Data = msg.Item2, Exception = ex});
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this._client.SendFailed(new OnSendFailedEventArgs {Data = "", Exception = ex});
                    this._client.Error(new OnErrorEventArgs {Exception = ex});
                }
            });
        }
    }
}
