namespace Loupedeck.TwitchPlugin
{
    using Microsoft.Extensions.Logging;
    using System;

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
        private readonly string _categoryName;

        public TwitchPluginLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // Create a new scope
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // Check if the log level is enabled
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);

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