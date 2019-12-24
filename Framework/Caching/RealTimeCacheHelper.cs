using System;
using System.Text;
using System.Collections.Generic;

namespace PmSoft.Caching
{
    /// <summary>
    /// 实时性缓存助手
    /// 主要有两个作用：递增缓存版本号、获取缓存CacheKey 
    /// </summary>
    [Serializable]
    public class RealTimeCacheHelper
    {
        /// <summary>
        /// 分区缓存版本字典
        /// </summary>
        private Dictionary<string, Dictionary<int, int>> areaVersionDictionary;
        /// <summary>
        /// 实体缓存版本字典
        /// </summary>
        private Dictionary<object, int> entityVersionDictionary;
        /// <summary>
        /// 全局缓存版本
        /// </summary>
        private int globalVersion;

        /// <summary>
        /// 构造函数 
        /// </summary>
        /// <param name="enableCache">是否启用缓存</param>
        /// <param name="typeHashID">类型名称哈希值</param>
        public RealTimeCacheHelper(bool enableCache, string typeHashID)
        {
            this.entityVersionDictionary = new Dictionary<object, int>();
            this.areaVersionDictionary = new Dictionary<string, Dictionary<int, int>>();
            this.EnableCache = enableCache;
            this.TypeHashID = typeHashID;
        }

        /// <summary>
        /// 获取列表缓存区域版本号
        /// </summary>
        /// <param name="propertyName">分区属性名称</param>
        /// <param name="propertyValue">分区属性值</param>
        /// <returns>分区属性的缓存版本（从0开始）</returns>
        public int GetAreaVersion(string propertyName, object propertyValue)
        {
            int num = 0;
            if (!string.IsNullOrEmpty(propertyName))
            {
                Dictionary<int, int> dictionary;
                propertyName = propertyName.ToLower();
                lock (this.areaVersionDictionary)
                {
                    if (this.areaVersionDictionary.TryGetValue(propertyName, out dictionary))
                    {
                        dictionary.TryGetValue(propertyValue.GetHashCode(), out num);
                    }
                }
            }
            return num;
        }

        /// <summary>
        /// 获取实体缓存的cacheKey 
        /// </summary>
        /// <param name="primaryKey">主键</param>
        /// <returns>实体的CacheKey</returns>
        public string GetCacheKeyOfEntity(object primaryKey)
        {
            if (ServiceLocator.GetService<ICacheService>().EnableDistributedCache)
            {
                return string.Concat(new object[] { this.TypeHashID, ":", primaryKey, ":", this.GetEntityVersion(primaryKey) });
            }
            return (this.TypeHashID + ":" + primaryKey);
        }

        /// <summary>
        /// 获取实体正文缓存的cacheKey 
        /// </summary>
        /// <param name="primaryKey">主键</param>
        /// <returns>实体正文缓存的cacheKey</returns>
        public string GetCacheKeyOfEntityBody(object primaryKey)
        {
            if (ServiceLocator.GetService<ICacheService>().EnableDistributedCache)
            {
                return string.Concat(new object[] { this.TypeHashID, ":B-", primaryKey, ":", this.GetEntityVersion(primaryKey) });
            }
            return (this.TypeHashID + ":B-" + primaryKey);
        }

        /// <summary>
        /// CacheTimelinessHelper的CacheKey 
        /// </summary>
        /// <param name="typeHashID"></param>
        /// <returns>typeHashID对应类型的缓存设置CacheKey</returns>
        internal static string GetCacheKeyOfTimelinessHelper(string typeHashID)
        {
            return ("CacheTimelinessHelper:" + typeHashID);
        }

        /// <summary>
        /// 获取Entity的缓存版本 
        /// </summary>
        /// <param name="primaryKey">实体主键</param>
        /// <returns>实体的缓存版本（从0开始）</returns>
        public int GetEntityVersion(object primaryKey)
        {
            int num = 0;
            lock (this.entityVersionDictionary)
                this.entityVersionDictionary.TryGetValue(primaryKey, out num);
            return num;
        }

        /// <summary>
        /// 列表缓存全局version 
        /// </summary>
        /// <returns></returns>
        public int GetGlobalVersion()
        {
            return this.globalVersion;
        }

        /// <summary>
        /// 获取列表缓存CacheKey的前缀（例如：abe3ds2sa90:8:） 
        /// </summary>
        /// <param name="cacheVersionType">列表缓存版本设置</param>
        /// <returns>列表缓存CacheKey的前缀</returns>
        public string GetListCacheKeyPrefix(CacheVersionType cacheVersionType)
        {
            return this.GetListCacheKeyPrefix(cacheVersionType, null, null);
        }

        /// <summary>
        /// 获取列表缓存CacheKey的前缀（例如：abe3ds2sa90:8:） 
        /// </summary>
        /// <param name="cacheVersionSetting">列表缓存设置</param>
        /// <returns>列表缓存CacheKey的前缀</returns>
        public string GetListCacheKeyPrefix(IListCacheSetting cacheVersionSetting)
        {
            StringBuilder builder = new StringBuilder(this.TypeHashID);
            builder.Append("-L:");
            switch (cacheVersionSetting.CacheVersionType)
            {
                case CacheVersionType.GlobalVersion:
                    builder.AppendFormat("{0}:", this.GetGlobalVersion());
                    break;

                case CacheVersionType.AreaVersion:
                    builder.AppendFormat("{0}-{1}-{2}:", cacheVersionSetting.AreaCachePropertyName, cacheVersionSetting.AreaCachePropertyValue.ToString(), this.GetAreaVersion(cacheVersionSetting.AreaCachePropertyName, cacheVersionSetting.AreaCachePropertyValue));
                    break;
            }
            return builder.ToString();
        }

        /// <summary>
        /// 获取列表缓存CacheKey的前缀（例如：abe3ds2sa90:8:） 
        /// </summary>
        /// <param name="cacheVersionType"></param>
        /// <param name="areaCachePropertyName">缓存分区名称</param>
        /// <param name="areaCachePropertyValue">缓存分区值</param>
        /// <returns></returns>
        public string GetListCacheKeyPrefix(CacheVersionType cacheVersionType, string areaCachePropertyName, object areaCachePropertyValue)
        {
            StringBuilder builder = new StringBuilder(this.TypeHashID);
            builder.Append("-L:");
            switch (cacheVersionType)
            {
                case CacheVersionType.GlobalVersion:
                    builder.AppendFormat("{0}:", this.GetGlobalVersion());
                    break;

                case CacheVersionType.AreaVersion:
                    builder.AppendFormat("{0}-{1}-{2}:", areaCachePropertyName, areaCachePropertyValue, this.GetAreaVersion(areaCachePropertyName, areaCachePropertyValue));
                    break;
            }
            return builder.ToString();
        }

        /// <summary>
        /// 递增列表缓存区域version 
        /// </summary>
        /// <param name="propertyName">分区属性名称</param>
        /// <param name="propertyValues">多个分区属性值</param>
        public void IncreaseAreaVersion(string propertyName, IEnumerable<object> propertyValues)
        {
            this.IncreaseAreaVersion(propertyName, propertyValues, true);
        }

        /// <summary>
        /// 递增列表缓存区域version
        /// </summary>
        /// <param name="propertyName">分区属性名称</param>
        /// <param name="propertyValue">分区属性值</param>
        public void IncreaseAreaVersion(string propertyName, object propertyValue)
        {
            if (propertyValue != null)
            {
                this.IncreaseAreaVersion(propertyName, new object[] { propertyValue }, true);
            }
        }

        /// <summary>
        /// 递增列表缓存区域version 
        /// </summary>
        /// <param name="propertyName">分区属性名称</param>
        /// <param name="propertyValues">多个分区属性值</param>
        /// <param name="raiseChangeEvent">是否触发Change事件</param>
        private void IncreaseAreaVersion(string propertyName, IEnumerable<object> propertyValues, bool raiseChangeEvent)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                Dictionary<int, int> dictionary;
                propertyName = propertyName.ToLower();
                int num = 0;
                lock (this.areaVersionDictionary)
                {
                    if (!this.areaVersionDictionary.TryGetValue(propertyName, out dictionary))
                    {
                        this.areaVersionDictionary[propertyName] = new Dictionary<int, int>();
                        dictionary = this.areaVersionDictionary[propertyName];
                    }

                    foreach (object propertyVal in propertyValues)
                    {
                        int hashCode = propertyVal.GetHashCode();
                        if (dictionary.TryGetValue(hashCode, out num))
                        {
                            num++;
                        }
                        else
                        {
                            num = 1;
                        }
                        dictionary[hashCode] = num;
                    }
                }
                if (raiseChangeEvent)
                {
                    this.OnChanged();
                }
            }
        }

        /// <summary>
        /// 递增实体缓存（仅更新实体时需要递增）
        /// </summary>
        /// <param name="entityId">实体Id</param>
        public void IncreaseEntityCacheVersion(object entityId)
        {
            if (ServiceLocator.GetService<ICacheService>().EnableDistributedCache)
            {
                int num;
                lock (this.entityVersionDictionary)
                {
                    if (this.entityVersionDictionary.TryGetValue(entityId, out num))
                    {
                        num++;
                    }
                    else
                    {
                        num = 1;
                    }
                    this.entityVersionDictionary[entityId] = num;
                }
                this.OnChanged();
            }
        }

        /// <summary>
        /// 递增列表缓存全局版本 
        /// </summary>
        public void IncreaseGlobalVersion()
        {
            this.globalVersion++;
        }

        /// <summary>
        /// 递增列表缓存version（增加、更改、删除实体时需要递增）
        /// </summary>
        /// <param name="entity"></param>
        public void IncreaseListCacheVersion(IEntity entity)
        {
            if (this.PropertiesOfArea != null)
            {
                foreach (EntityPropertyInfo info in this.PropertiesOfArea)
                {
                    object obj = info.GetValue(entity, null);
                    if (obj != null)
                    {
                        this.IncreaseAreaVersion(info.Name.ToLower(), new object[] { obj }, false);
                    }
                }
            }
            this.IncreaseGlobalVersion();
            this.OnChanged();
        }

        /// <summary>
        /// 标识为已删除
        /// </summary>
        /// <param name="entity"></param>
        public void MarkDeletion(IEntity entity)
        {
            ICacheService service = ServiceLocator.GetService<ICacheService>();
            if (this.EnableCache)
            {
                service.MarkDeletionAsync(this.GetCacheKeyOfEntity(entity.EntityId), entity, CachingExpirationType.SingleObject).Wait();
            }
        }

        /// <summary>
        /// 对象变更时回调 
        /// 在分布式缓存情况，需要把缓存设置存储到缓存中 
        /// </summary>
        private void OnChanged()
        {
            ICacheService service = ServiceLocator.GetService<ICacheService>();
            if (service.EnableDistributedCache)
            {
                service.SetAsync(GetCacheKeyOfTimelinessHelper(this.TypeHashID), this, CachingExpirationType.Invariable).Wait();
            }
        }

        /// <summary>
        /// 缓存过期类型 
        /// </summary>
        public CachingExpirationType CachingExpirationType { get; set; }

        /// <summary>
        /// 是否使用缓存
        /// </summary>
        public bool EnableCache { get; private set; }

        /// <summary>
        /// 缓存分区的属性
        /// </summary>
        public IEnumerable<EntityPropertyInfo> PropertiesOfArea { get; set; }

        /// <summary>
        /// 实体正文缓存对应的属性名称（如果不需单独存储实体正文缓存，则不要设置该属性）
        /// </summary>
        public EntityPropertyInfo PropertyNameOfBody { get; set; }

        /// <summary>
        /// 完整名称md5-16 
        /// </summary>
        public string TypeHashID { get; private set; }
    }
}
