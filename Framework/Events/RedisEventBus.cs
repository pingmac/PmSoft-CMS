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
    public class RedisEventBus : IEventBus
    {
        private readonly IRedisCacheClient redisClient;
        private const string EventsCacheKey = "Events";

        public RedisEventBus(IRedisCacheClient redisClient)
        {
            this.redisClient = redisClient;
        }

        #region 私有方法

        private string GetChannelName<TEventArgs>(string handlerName)
        {
            string eventKey = GetEventKey<TEventArgs>();
            return $"RedisEventBus-{handlerName}-{eventKey}";
        }

        private string GetEventKey<TEventArgs>()
        {
            return typeof(TEventArgs).FullName;
        }

        private async Task<bool> AddSubscriptionAsync<TEventArgs, TEventHandler>()
        {
            var eventKey = GetEventKey<TEventArgs>();

            var handlerType = typeof(TEventHandler);
            if (await redisClient.Db0.HashExistsAsync(EventsCacheKey, eventKey))
            {
                var handlers = await redisClient.Db0.HashGetAsync<List<string>>(EventsCacheKey, eventKey);

                if (handlers.Contains(handlerType.FullName))
                {
                    return false;
                }
                handlers.Add(handlerType.FullName);
                return await redisClient.Db0.HashSetAsync<List<string>>(EventsCacheKey, eventKey, handlers);
            }
            else
            {
                return await redisClient.Db0.HashSetAsync<List<string>>(EventsCacheKey, eventKey, new List<string> { handlerType.FullName });
            }
        }

        private async Task<List<string>> GetEventHandlersAsync<TEventArgs>()
        {
            var eventKey = GetEventKey<TEventArgs>();
            return await redisClient.Db0.HashGetAsync<List<string>>(EventsCacheKey, eventKey);
        }

        private async Task<bool> HasSubscriptionsForEventAsync<TEventArgs>()
        {
            var eventKey = GetEventKey<TEventArgs>();
            return await redisClient.Db0.HashExistsAsync(EventsCacheKey, eventKey);
        }

        #endregion

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public void Subscribe<TEventArgs, TEventHandler>()
            where TEventArgs : CommonEventArgs
            where TEventHandler : IEventHandler<CommonEventArgs>
        {
            AddSubscriptionAsync<TEventArgs, TEventHandler>().Wait();

            var channelName = GetChannelName<TEventArgs>(typeof(TEventHandler).FullName);

            redisClient.Db0.SubscribeAsync<TEventArgs>(channelName, async (eventArgs) =>
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
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        public async Task<bool> PublishAsync<TEventArgs>(TEventArgs eventArgs)
            where TEventArgs : CommonEventArgs
        {
            if (!await HasSubscriptionsForEventAsync<TEventArgs>())
                return false;

            var handlerNames = await GetEventHandlersAsync<TEventArgs>();
            foreach (var handlerName in handlerNames)
            {
                var handlerChannelName = GetChannelName<TEventArgs>(handlerName);
                await redisClient.Db0.PublishAsync<TEventArgs>(handlerChannelName, eventArgs);
            }
            return true;
        }

    }
}
