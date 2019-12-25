using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using StackExchange.Redis.Extensions.Core.Abstractions;
using PmSoft.Logging;

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

        /// <summary>
        /// 获取通道名
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="handlerName"></param>
        /// <returns></returns>
        private string GetChannelName<TSender, TEventArgs>(string handlerName)
        {
            string eventKey = GetEventKey<TSender, TEventArgs>();
            return $"RedisEventBus-{handlerName}-{eventKey}";
        }

        /// <summary>
        /// 获取事件KEY
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <returns></returns>
        private string GetEventKey<TSender, TEventArgs>()
        {
            return typeof(TSender).FullName + "-" + typeof(TEventArgs).FullName;
        }

        private async Task<bool> AddSubscriptionAsync<TSender, TEventArgs, TEventHandler>()
        {
            var eventKey = GetEventKey<TSender, TEventArgs>();

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

        private async Task<List<string>> GetEventHandlersAsync<TSender, TEventArgs>()
        {
            var eventKey = GetEventKey<TSender, TEventArgs>();
            return await redisClient.Db0.HashGetAsync<List<string>>(EventsCacheKey, eventKey);
        }

        private async Task<bool> HasSubscriptionsForEventAsync<TSender, TEventArgs>()
        {
            var eventKey = GetEventKey<TSender, TEventArgs>();
            return await redisClient.Db0.HashExistsAsync(EventsCacheKey, eventKey);
        }

        #endregion

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public void Subscribe<TSender, TEventArgs, TEventHandler>()
            where TEventArgs : CommonEventArgs
            where TEventHandler : IEventHandler<TSender, TEventArgs>
        {
            AddSubscriptionAsync<TSender, TEventArgs, TEventHandler>().Wait();

            var channelName = GetChannelName<TSender, TEventArgs>(typeof(TEventHandler).FullName);

            redisClient.Db0.SubscribeAsync<RedisEventMessgae<TSender, TEventArgs>>(channelName, async (message) =>
            {
                var handler = ServiceLocator.GetService<TEventHandler>();
                if (handler != null && message != null)
                {
                    await handler.HandleAsync(message.Sender, message.EventArgs).ContinueWith(task =>
                    {
                        task.Exception?.Handle(exception =>
                        {
                            LoggerLocator.GetLogger<RedisEventBus>().LogError(exception, "执行触发操作事件时发生异常");
                            return true;
                        });
                    }).ConfigureAwait(false);
                }
            });
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        public async Task<bool> PublishAsync<TSender, TEventArgs>(TSender sender, TEventArgs eventArgs)
            where TEventArgs : CommonEventArgs
        {
            if (!await HasSubscriptionsForEventAsync<TSender, TEventArgs>())
                return false;

            var messgae = new RedisEventMessgae<TSender, TEventArgs>(sender, eventArgs);

            var handlerNames = await GetEventHandlersAsync<TSender, TEventArgs>();
            foreach (var handlerName in handlerNames)
            {
                var handlerChannelName = GetChannelName<TSender, TEventArgs>(handlerName);
                await redisClient.Db0.PublishAsync<RedisEventMessgae<TSender, TEventArgs>>(handlerChannelName, messgae);
            }
            return true;
        }

        [Serializable]
        public class RedisEventMessgae<TSender, TEventArgs>
        {
            public RedisEventMessgae() { }

            public RedisEventMessgae(TSender sender, TEventArgs eventArgs)
            {
                Sender = sender;
                EventArgs = eventArgs;
            }

            public TSender Sender { get; set; }

            public TEventArgs EventArgs { get; set; }
        }
    }
 
}
