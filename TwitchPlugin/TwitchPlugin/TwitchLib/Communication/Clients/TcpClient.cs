namespace TwitchLib.Communication.Clients
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using Events;
    using Interfaces;
    using Models;
    using Services;

    using Loupedeck;

    public class TcpClient : IClient
    {
        public TimeSpan DefaultKeepAliveInterval { get; set; }
        public Int32 SendQueueLength => this._throttlers.SendQueue.Count;
        public Int32 WhisperQueueLength => this._throttlers.WhisperQueue.Count;
        public Boolean IsConnected => this.Client?.Connected ?? false;
        public IClientOptions Options { get; }

        public event EventHandler<OnConnectedEventArgs> OnConnected;
        public event EventHandler<OnDataEventArgs> OnData;
        public event EventHandler<OnDisconnectedEventArgs> OnDisconnected;
        public event EventHandler<OnErrorEventArgs> OnError;
        public event EventHandler<OnFatalErrorEventArgs> OnFatality;
        public event EventHandler<OnMessageEventArgs> OnMessage;
        public event EventHandler<OnMessageThrottledEventArgs> OnMessageThrottled;
        public event EventHandler<OnWhisperThrottledEventArgs> OnWhisperThrottled;
        public event EventHandler<OnSendFailedEventArgs> OnSendFailed;
        public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
        public event EventHandler<OnReconnectedEventArgs> OnReconnected;

        private readonly String _server = "irc.chat.twitch.tv";
        private Int32 Port => this.Options != null ? this.Options.UseSsl ? 443 : 80 : 0;
        public System.Net.Sockets.TcpClient Client { get; private set; }
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly Throttlers _throttlers;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private Boolean _stopServices;
        private Boolean _networkServicesRunning;
        private Task[] _networkTasks;
        private Task _monitorTask;

        public TcpClient(IClientOptions options = null)
        {
            this.Options = options ?? new ClientOptions();
            this._throttlers =
                new Throttlers(this, this.Options.ThrottlingPeriod, this.Options.WhisperThrottlingPeriod)
                {
                    TokenSource = this._tokenSource
                };
            this.InitializeClient();
        }

        private void InitializeClient()
        {
            this.Client = new System.Net.Sockets.TcpClient();

            if (this._monitorTask == null)
            {
                this._monitorTask = this.StartMonitorTask();
                return;
            }

            if (this._monitorTask.IsCompleted) this._monitorTask = this.StartMonitorTask();
        }

        public Boolean Open()
        {
            try
            {
                if (this.IsConnected) return true;

                Helpers.StartNewTask(() => { 
                this.InitializeClient();
                this.Client.Connect(this._server, this.Port);
                if (this.Options.UseSsl)
                {
                    var ssl = new SslStream(this.Client.GetStream(), false);
                    ssl.AuthenticateAsClient(this._server);
                    this._reader = new StreamReader(ssl);
                    this._writer = new StreamWriter(ssl);
                }
                else
                {
                    this._reader = new StreamReader(this.Client.GetStream());
                    this._writer = new StreamWriter(this.Client.GetStream());
                }
                }).Wait(10000);

                if (!this.IsConnected) return this.Open();
                
                this.StartNetworkServices();
                return true;

            }
            catch (Exception)
            {
                this.InitializeClient();
                return false;
            }
        }

        public void Close(Boolean callDisconnect = true)
        {
            this._reader?.Dispose();
            this._writer?.Dispose();
            this.Client?.Close();

            this._stopServices = callDisconnect;
            this.CleanupServices();
            this.InitializeClient();
            this.OnDisconnected?.Invoke(this, new OnDisconnectedEventArgs());
        }

        public void Reconnect()
        {
            this.Close();
            this.Open();
            this.OnReconnected?.Invoke(this, new OnReconnectedEventArgs());
        }

        public Boolean Send(String message)
        {
            try
            {
                if (!this.IsConnected || this.SendQueueLength >= this.Options.SendQueueCapacity)
                {
                    return false;
                }

                this._throttlers.SendQueue.Add(new Tuple<DateTime, String>(DateTime.UtcNow, message));

                return true;
            }
            catch (Exception ex)
            {
                this.OnError?.Invoke(this, new OnErrorEventArgs {Exception = ex});
                throw;
            }
        }

        public Boolean SendWhisper(String message)
        {
            try
            {
                if (!this.IsConnected || this.WhisperQueueLength >= this.Options.WhisperQueueCapacity)
                {
                    return false;
                }

                this._throttlers.WhisperQueue.Add(new Tuple<DateTime, String>(DateTime.UtcNow, message));

                return true;
            }
            catch (Exception ex)
            {
                this.OnError?.Invoke(this, new OnErrorEventArgs {Exception = ex});
                throw;
            }
        }

        private void StartNetworkServices()
        {
            this._networkServicesRunning = true;
            this._networkTasks = new[]
            {
                this.StartListenerTask(),
                this._throttlers.StartSenderTask(),
                this._throttlers.StartWhisperSenderTask()
            }.ToArray();

            if (!this._networkTasks.Any(c => c.IsFaulted)) return;
            this._networkServicesRunning = false;
            this.CleanupServices();
        }

        public Task SendAsync(String message)
        {
            return Task.Run(async () =>
            {
                await this._writer.WriteLineAsync(message);
                await this._writer.FlushAsync();
            });
        }

        private Task StartListenerTask()
        {
            return Task.Run(async () =>
            {
                while (this.IsConnected && this._networkServicesRunning)
                {
                    try
                    {
                        var input = await this._reader.ReadLineAsync();
                        this.OnMessage?.Invoke(this, new OnMessageEventArgs {Message = input});
                    }
                    catch (Exception ex)
                    {
                        this.OnError?.Invoke(this, new OnErrorEventArgs {Exception = ex});
                    }
                }
            });
        }

        private Task StartMonitorTask()
        {
            return Task.Run(() =>
            {
                var needsReconnect = false;
                try
                {
                    var lastState = this.IsConnected;
                    while (!this._tokenSource.IsCancellationRequested)
                    {
                        if (lastState == this.IsConnected)
                        {
                            Thread.Sleep(200);
                            continue;
                        }
                        this.OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { IsConnected = this.IsConnected, WasConnected = lastState });

                        if (this.IsConnected)
                            this.OnConnected?.Invoke(this, new OnConnectedEventArgs());

                        if (!this.IsConnected && !this._stopServices)
                        {
                            if (lastState && this.Options.ReconnectionPolicy != null && !this.Options.ReconnectionPolicy.AreAttemptsComplete())
                            {
                                needsReconnect = true;
                                break;
                            }
                            this.OnDisconnected?.Invoke(this, new OnDisconnectedEventArgs());
                        }

                        lastState = this.IsConnected;
                    }
                }
                catch (Exception ex)
                {
                    this.OnError?.Invoke(this, new OnErrorEventArgs {Exception = ex});
                }

                if (needsReconnect && !this._stopServices)
                    this.Reconnect();
            }, this._tokenSource.Token);
        }

        private void CleanupServices()
        {
            this._tokenSource.Cancel();
            this._tokenSource = new CancellationTokenSource();
            this._throttlers.TokenSource = this._tokenSource;

            if (!this._stopServices) return;
            if (!(this._networkTasks?.Length > 0)) return;
            if (Task.WaitAll(this._networkTasks, 15000)) return;

            this.OnFatality?.Invoke(this,
                new OnFatalErrorEventArgs
                {
                    Reason = "Fatal network error. Network services fail to shut down."
                });
            this._stopServices = false;
            this._throttlers.Reconnecting = false;
            this._networkServicesRunning = false;
        }

        public void WhisperThrottled(OnWhisperThrottledEventArgs eventArgs)
        {
            this.OnWhisperThrottled?.Invoke(this, eventArgs);
        }

        public void MessageThrottled(OnMessageThrottledEventArgs eventArgs)
        {
            this.OnMessageThrottled?.Invoke(this, eventArgs);
        }

        public void SendFailed(OnSendFailedEventArgs eventArgs)
        {
            this.OnSendFailed?.Invoke(this, eventArgs);
        }

        public void Error(OnErrorEventArgs eventArgs)
        {
            this.OnError?.Invoke(this, eventArgs);
        }

        public void Dispose()
        {
            this._reader?.Dispose();
            this._writer?.Dispose();
            this.Client?.Close();

            this._stopServices = true;
            this.CleanupServices();
            this._throttlers.ShouldDispose = true;
            this._tokenSource.Cancel();
            this._tokenSource.Token.WaitHandle.WaitOne(500);
            this._tokenSource.Dispose();
            this.Client?.Dispose();
            GC.Collect();
        }
    }
}
