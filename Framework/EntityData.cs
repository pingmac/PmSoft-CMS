using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using PmSoft.Caching;
using PmSoft.Utilities;

namespace PmSoft
{
    /// <summary>
    /// 实体元数据
    /// </summary>
    public class EntityData
    {
        private static ConcurrentDictionary<Type, EntityData> entityDatas;
        private RealTimeCacheHelper realTimeCacheHelper;

        public EntityData(Type t)
        {
            this.Type = t;
            this.TypeHashID = EncryptionUtility.MD5_16(t.FullName);
            ICacheService service = ServiceLocator.GetService<ICacheService>();

            //对realTimeCacheHelper进行初始化
            if (!service.EnableDistributedCache)
            {
                RealTimeCacheHelper helper = this.ParseCacheTimelinessHelper(t);
                this.realTimeCacheHelper = helper;
                service.SetAsync(RealTimeCacheHelper.GetCacheKeyOfTimelinessHelper(this.TypeHashID), helper, CachingExpirationType.Invariable).Wait();
            }
        }

        static EntityData()
        {
            entityDatas = new ConcurrentDictionary<Type, EntityData>();
        }

        /// <summary>
        /// 根据实体类型获取实体元数据
        /// </summary>
        /// <param name="t">实体类型</param>
        /// <returns></returns>
        public static EntityData ForType(Type type)
        {
            EntityData entityData;
            if (!entityDatas.TryGetValue(type, out entityData) && (entityData == null))
            {
                entityData = new EntityData(type);
                entityDatas[type] = entityData;
            }
            return entityData;
        }

        /// <summary>
        /// 解析Type的CacheTimelinessHelper 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private RealTimeCacheHelper ParseCacheTimelinessHelper(Type t)
        {
            RealTimeCacheHelper helper = null;
            object[] customAttributes = t.GetCustomAttributes(typeof(CacheSettingAttribute), true);
            if (customAttributes.Length > 0)
            {
                CacheSettingAttribute attribute = customAttributes[0] as CacheSettingAttribute;
                if (attribute != null)
                {
                    helper = new RealTimeCacheHelper(attribute.EnableCache, this.TypeHashID);
                    if (attribute.EnableCache)
                    {
                        switch (attribute.ExpirationPolicy)
                        {
                            case EntityCacheExpirationPolicies.Stable:
                                helper.CachingExpirationType = CachingExpirationType.Stable;
                                break;

                            case EntityCacheExpirationPolicies.Usual:
                                helper.CachingExpirationType = CachingExpirationType.UsualSingleObject;
                                break;

                            default:
                                helper.CachingExpirationType = CachingExpirationType.SingleObject;
                                break;
                        }
                        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance;
                        List<EntityPropertyInfo> list = new List<EntityPropertyInfo>();
                        if (!string.IsNullOrEmpty(attribute.PropertyNamesOfArea))
                        {
                            foreach (string str in attribute.PropertyNamesOfArea.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                PropertyInfo property = t.GetProperty(str, bindingFlags);
                                if (property != null)
                                {
                                    list.Add(property.AsEntityPropertyInfo());
                                }
                            }
                        }
                        helper.PropertiesOfArea = list;
                        if (!string.IsNullOrEmpty(attribute.PropertyNameOfBody))
                        {
                            PropertyInfo propertyInfo = t.GetProperty(attribute.PropertyNameOfBody, bindingFlags);
                            helper.PropertyNameOfBody = propertyInfo.AsEntityPropertyInfo();
                        }
                    }
                }
            }
            if (helper == null)
            {
                helper = new RealTimeCacheHelper(true, this.TypeHashID);
            }
            return helper;
        }

        /// <summary>
        /// 实体缓存设置 
        /// </summary>
        public RealTimeCacheHelper RealTimeCacheHelper
        {
            get
            {
                ICacheService service = ServiceLocator.GetService<ICacheService>();
                if (!service.EnableDistributedCache)
                {
                    return this.realTimeCacheHelper;
                }
                //从分布式缓存获取RealTimeCacheHelper对象
                string cacheKeyOfTimelinessHelper = RealTimeCacheHelper.GetCacheKeyOfTimelinessHelper(this.TypeHashID);
                RealTimeCacheHelper cacheHelper = service.GetFromFirstLevelAsync<RealTimeCacheHelper>(cacheKeyOfTimelinessHelper).Result;
                if (cacheHelper == null)
                {
                    cacheHelper = this.ParseCacheTimelinessHelper(this.Type);
                    service.SetAsync(cacheKeyOfTimelinessHelper, cacheHelper, CachingExpirationType.Invariable).Wait();
                }
                return cacheHelper;
            }
        }

        /// <summary>
        /// 实体类型
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// 类型的哈希值（16位md5） 
        /// </summary>
        public string TypeHashID { get; private set; }
    }
}

