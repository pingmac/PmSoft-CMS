using System;
using System.IO;
using System.Reflection;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using log4net;
using log4net.Config;

namespace PmSoft.Log4Net
{
    public class Log4NetLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, Log4NetLogger> _loggers = new ConcurrentDictionary<string, Log4NetLogger>();

        public Log4NetLoggerProvider(IOptions<Log4NetLoggerOptions> options)
            : this(options.Value.ConfigFilePath) { }

        public Log4NetLoggerProvider(string configFilePath)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(configFilePath));
        }

        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, CreateLoggerImplementation);
        }

        private Log4NetLogger CreateLoggerImplementation(string name)
        {
            return new Log4NetLogger(name);
        }

        public void Dispose()
        {

        }


    }
}
