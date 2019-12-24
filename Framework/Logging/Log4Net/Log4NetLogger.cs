using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using log4net;

namespace PmSoft.Log4Net
{
    public class Log4NetLogger : ILogger
    {
        private ILog log;

        public Log4NetLogger(string _name)
        {
            Name = _name;
            this.log = LogManager.GetLogger(Assembly.GetEntryAssembly(), _name);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message) && exception == null)
                return;

            switch (logLevel)
            {
                case LogLevel.Debug:
                    this.log.Debug(message, exception);
                    return;

                case LogLevel.Information:
                    this.log.Info(message, exception);
                    return;

                case LogLevel.Warning:
                    this.log.Warn(message, exception);
                    return;

                case LogLevel.Error:
                    this.log.Error(message, exception);
                    return;
            }

        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return this.log.IsDebugEnabled;

                case LogLevel.Information:
                    return this.log.IsInfoEnabled;

                case LogLevel.Warning:
                    return this.log.IsWarnEnabled;

                case LogLevel.Error:
                    return this.log.IsErrorEnabled;
            }
            return false;
        }

        public string Name { get; }

        internal IExternalScopeProvider ScopeProvider { get; set; }

        public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state);
    }
}
