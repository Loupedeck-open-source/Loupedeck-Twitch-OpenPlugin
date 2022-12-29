namespace TwitchLib.Communication.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Enums;
    using Events;
    using Interfaces;
    using Models;
    using Services;

    public class WebSocketClient : IClient
    {
        public TimeSpan DefaultKeepAliveInterval { get; set; }
        public Int32 SendQueueLength => this._throttlers.SendQueue.Count;
        public Int32 WhisperQueueLength => this._throttlers.WhisperQueue.Count;
        public Boolean IsConnected => this.Client?.State == WebSocketState.Open;
        public IClientOptions Options { get; }
        public ClientWebSocket Client { get; private set; }

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

        private String Url { get; }
        private readonly Throttlers _throttlers;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private Boolean _stopServices;
        private Boolean _networkServicesRunning;
        private Task[] _networkTasks;
        private Task _monitorTask;
        
        public WebSocketClient(IClientOptions options = null)
        {
            this.Options = options ?? new ClientOptions();

            switch (this.Options.ClientType)
            {
                case ClientType.Chat:
                    this.Url = this.Options.UseSsl ? "wss://irc-ws.chat.twitch.tv:443" : "ws://irc-ws.chat.twitch.tv:80";
                    break;
                case ClientType.PubSub:
                    this.Url = this.Options.UseSsl ? "wss://pubsub-edge.twitch.tv:443" : "ws://pubsub-edge.twitch.tv:80";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this._throttlers = new Throttlers(this, this.Options.ThrottlingPeriod, this.Options.WhisperThrottlingPeriod) { TokenSource = this._tokenSource };
        }

        private void InitializeClient()
        {
            this.Client?.Abort();
            this.Client = new ClientWebSocket();
            this._monitorTask = this.StartMonitorTask();
        }

        public Boolean Open()
        {
            try
            {
                if (this.IsConnected) return true;

                this.InitializeClient();
                this.Client.ConnectAsync(new Uri(this.Url), this._tokenSource.Token).Wait(10000);
                if (!this.IsConnected) return this.Open();

                this.StartNetworkServices();
                return true;
            }
            //catch (WebSocketException ex)
            //catch (System.Net.WebException ex)
            catch (Exception ex) 
            {
                //this.InitializeClient();
                this.OnError?.Invoke(this, new OnErrorEventArgs { Exception = ex });
                return false;
            }
        }

        public void Close(Boolean callDisconnect = true)
        {
            this.Client?.Abort();
            this._stopServices = callDisconnect;
            this.CleanupServices();
            //this.InitializeClient();
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
                this.OnError?.Invoke(this, new OnErrorEventArgs { Exception = ex });
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
                this.OnError?.Invoke(this, new OnErrorEventArgs { Exception = ex });
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

        public Task SendAsync(Byte[] message)
        {
            return this.Client.SendAsync(new ArraySegment<Byte>(message), WebSocketMessageType.Text, true, this._tokenSource.Token);
        }

        private Task StartListenerTask()
        {
            return Task.Run(async () =>
            {
                var message = "";

                while (this.IsConnected && this._networkServicesRunning && !this._tokenSource.IsCancellationRequested)
                {
                    WebSocketReceiveResult result;
                    var buffer = new Byte[1024];

                    try
                    {
                        result = await this.Client.ReceiveAsync(new ArraySegment<Byte>(buffer), this._tokenSource.Token);
                    }
                    catch(Exception ex)
                    {
                        //this.InitializeClient();
                        this.OnError?.Invoke(this, new OnErrorEventArgs { Exception = ex });
                        break;
                    }

                    if (result == null) continue;

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            this.Close();
                            break;
                        case WebSocketMessageType.Text when !result.EndOfMessage:
                            message += Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                            continue;
                        case WebSocketMessageType.Text:
                            message += Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                            this.OnMessage?.Invoke(this, new OnMessageEventArgs(){Message = message});
                            break;
                        case WebSocketMessageType.Binary:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    message = "";
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
                        this.OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { IsConnected = this.Client.State == WebSocketState.Open, WasConnected = lastState});

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
                            if (this.Client.CloseStatus != null && this.Client.CloseStatus != WebSocketCloseStatus.NormalClosure)
                                this.OnError?.Invoke(this, new OnErrorEventArgs { Exception = new Exception(this.Client.CloseStatus + " " + this.Client.CloseStatusDescription) });
                        }

                        lastState = this.IsConnected;
                    }
                }
                catch (Exception ex)
                {
                    this.OnError?.Invoke(this, new OnErrorEventArgs { Exception = ex });
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
            if (Task.WaitAll(this._networkTasks, 5000)) return;

            this._networkTasks = new List<Task>().ToArray();
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
            this.Client?.Abort();
            this.CleanupServices();
            this._stopServices = true;
            this._throttlers.ShouldDispose = true;
            this._tokenSource.Cancel();
            this._tokenSource.Token.WaitHandle.WaitOne(500);
            this._tokenSource.Dispose();
            this.Client?.Dispose();
            GC.Collect();
        }
    }
}
