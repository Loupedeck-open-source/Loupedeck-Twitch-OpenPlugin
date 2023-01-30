namespace Loupedeck.TwitchPlugin
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.ComponentModel;

    public class TwitchPluginLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName) => new TwitchPluginLogger(categoryName);

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class TwitchPluginLogger : ILogger
    {
        private readonly String _categoryName;
        private static UInt32 _intstance_id = 0;
        private UInt32 _thisLoggerId = TwitchPluginLogger._intstance_id++;
        public TwitchPluginLogger(String categoryName) => this._categoryName = categoryName;

        public IDisposable BeginScope<TState>(TState state) =>
            // Create a new scope
            null;

        public Boolean IsEnabled(LogLevel logLevel) =>
            // Check if the log level is enabled
            true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, String> formatter)
        {
            var message = $"{this._thisLoggerId}:" + formatter(state, exception);

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    TwitchPlugin.PluginLog.Verbose(message);
                    break;
                case LogLevel.Information:
                    TwitchPlugin.PluginLog.Info(message);
                    break;
                case LogLevel.Warning:
                    TwitchPlugin.PluginLog.Warning(message);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                default:
                    if(exception!=null)
                    {
                        TwitchPlugin.PluginLog.Error(exception, message);
                    }
                    else
                    {
                        TwitchPlugin.PluginLog.Error(message);
                    }
                    break;
            }
        }
    }
}