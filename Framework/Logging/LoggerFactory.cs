using System;
using PmSoft;
using PmSoft.Utilities;
using Microsoft.Extensions.Logging;

namespace PmSoft.Logging
{

    /// <summary>
    /// 系统日志工厂 
    /// </summary>
    public static class LoggerFactory
    {

        public static ILogger<T> GetLogger<T>()
        {
            return ServiceLocator.GetService<ILoggerFactory>().CreateLogger<T>();
        }


        public static ILogger GetLogger(Type type)
        {
            return ServiceLocator.GetService<ILoggerFactory>().CreateLogger(type);
        }
    }
}

