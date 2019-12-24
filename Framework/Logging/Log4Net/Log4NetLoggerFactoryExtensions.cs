using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace PmSoft.Log4Net
{
    public static class Log4NetLoggerFactoryExtensions
    {
        public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, Log4NetLoggerProvider>());
            return builder;
        }

        public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder, Action<Log4NetLoggerOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddLog4Net();
            builder.Services.Configure(configure);

            return builder;
        }

        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory)
        {
            factory.AddLog4Net("Config/log4net.config");
            return factory;
        }

        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, string configFilePath)
        {
            factory.AddProvider(new Log4NetLoggerProvider(configFilePath));
            return factory;
        }
    }
}
