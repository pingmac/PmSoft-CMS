using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace PmSoft.Events
{
    /// <summary>
    /// 基于Redis的分布式事件总线
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    public class RedisEventBus<TEventArgs> : IEventBus<TEventArgs> where TEventArgs : CommonEventArgs
    {
        private static readonly IRedisCacheClient cacheClient;
        private static volatile RedisEventBus<TEventArgs> instance;
        private static readonly object lockObj;
        private const string EventsCacheKey = "Events";

        static RedisEventBus()
        {
            instance = null;
            cacheClient = ServiceLocator.GetService<IRedisCacheClient>();
            lockObj = new object();
        }

        /// <summary>
        /// 获取实例 
        /// </summary>
        /// <returns></returns>
        public static RedisEventBus<TEventArgs> Instance()
        {
            if (instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = new RedisEventBus<TEventArgs>();
                    }
                }
            }
            return instance;
        }

        #region 私有方法

        private string GetChannelName(string handlerName)
        {
            string eventKey = GetEventKey();
            return $"RedisEventBus-{handlerName}-{eventKey}";
        }

        private string GetEventKey()
        {
            return typeof(TEventArgs).FullName;
        }

        private async Task<bool> AddSubscriptionAsync<TEventHandler>()
        {
            var eventKey = GetEventKey();

            var handlerType = typeof(TEventHandler);
            if (await cacheClient.Db0.HashExistsAsync(EventsCacheKey, eventKey))
            {
                var handlers = await cacheClient.Db0.HashGetAsync<List<string>>(EventsCacheKey, eventKey);

                if (handlers.Contains(handlerType.FullName))
                {
                    return false;
                }
                handlers.Add(handlerType.FullName);
                return await cacheClient.Db0.HashSetAsync<List<string>>(EventsCacheKey, eventKey, handlers);
            }
            else
            {
                return await cacheClient.Db0.HashSetAsync<List<string>>(EventsCacheKey, eventKey, new List<string> { handlerType.FullName });
            }
        }

        private async Task<List<string>> GetEventHandlersAsync()
        {
            var eventKey = GetEventKey();
            return await cacheClient.Db0.HashGetAsync<List<string>>(EventsCacheKey, eventKey);
        }

        private async Task<bool> HasSubscriptionsForEventAsync()
        {
            var eventKey = GetEventKey();
            return await cacheClient.Db0.HashExistsAsync(EventsCacheKey, eventKey);
        }

        #endregion

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="TEventHandler">事件处理类</typeparam>
        public void Subscribe<TEventHandler>()
            where TEventHandler : IEventHandler<CommonEventArgs>
        {
            AddSubscriptionAsync<TEventHandler>().Wait();

            var channelName = GetChannelName(typeof(TEventHandler).FullName);

            cacheClient.Db0.SubscribeAsync<TEventArgs>(channelName, async (eventArgs) =>
            {
                var handler = ServiceLocator.GetService<TEventHandler>();
                if (null != handler)
                {
                    await handler.HandleAsync(eventArgs).ConfigureAwait(false);
                }
            });
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="eventArgs">事件参数</param>
        /// <returns></returns>
        public async Task<bool> PublishAsync(TEventArgs eventArgs)
        {
            if (!await HasSubscriptionsForEventAsync())
                return false;

            var handlerNames = await GetEventHandlersAsync();
            foreach (var handlerName in handlerNames)
            {
                var handlerChannelName = GetChannelName(handlerName);
                await cacheClient.Db0.PublishAsync<TEventArgs>(handlerChannelName, eventArgs);
            }
            return true;
        }

    }
}
