using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PmSoft.Logging
{
    public static class LoggerExtensions
    {

        public static void LogDebug(this ILogger logger, Exception exception, string message)
        {
            logger.LogDebug(exception, message);
        }

        public static void LogDebug(this ILogger logger, string message)
        {
            logger.LogDebug(message);
        }

        public static void LogError(this ILogger logger, string message)
        {
            logger.LogError(message);
        }

        public static void LogError(this ILogger logger, Exception exception, string message)
        {
            logger.LogError(exception, message);
        }

        public static void LogInformation(this ILogger logger, Exception exception, string message)
        {
            logger.LogInformation(exception, message);
        }

        public static void LogInformation(this ILogger logger, string message)
        {
            logger.LogInformation(message);
        }

        public static void LogWarning(this ILogger logger, string message)
        {
            logger.LogWarning(message);
        }

        public static void LogWarning(this ILogger logger, Exception exception, string message)
        {
            logger.LogWarning(exception, message);
        }
    }
}
