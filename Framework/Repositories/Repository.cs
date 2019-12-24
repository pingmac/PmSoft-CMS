using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using PetaPoco;
using PmSoft.Caching;
using System.Threading.Tasks;
using PetaPoco.Internal;
using MySql.Data.MySqlClient;
using PmSoft.DBContext;
using PetaPoco.Core;

namespace PmSoft.Repositories
{

    public class Repository<TEntity> : Repository<TEntity, DefaultDbContext> where TEntity : class, IEntity { }

    public class Repository<TEntity, TDbContext> : IRepository<TEntity>
        where TEntity : class, IEntity
        where TDbContext : PetaPocoDbContext
    {
        public readonly ICacheService cacheService;

        /// <summary>
        /// 可缓存的列表缓存页数
        /// </summary>
        private int cacheablePageCount;
        /// <summary>
        /// 主流查询最大返回记录数
        /// </summary>
        private long primaryMaxRecords;
        /// <summary>
        /// 非主流查询最大返回记录数
        /// </summary>
        private int secondaryMaxRecords;

        public Repository()
        {
            this.cacheService = ServiceLocator.GetService<ICacheService>();
            this.cacheablePageCount = 30;
            this.primaryMaxRecords = 50000;
            this.secondaryMaxRecords = 5000;
        }

        /// <summary>
        /// 默认PetaPocoDatabase实例
        /// </summary>
        /// <returns></returns>
        protected virtual IDatabase CreateDAO()
        {
            return ServiceLocator.GetDBContext<TDbContext>();
        }


        #region Insert,Update,Delete
        /// <summary>
        /// 把实体entity添加到数据库
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<object> InsertAsync(TEntity entity)
        {
            object obj = await CreateDAO().InsertAsync(entity);
            await this.OnInserted(entity);
            return obj;
        }


        /// <summary>
        /// 数据库新增实体后自动调用该方法
        /// </summary>
        /// <param name="entity"></param>
        protected virtual async Task OnInserted(TEntity entity)
        {
            if (RealTimeCacheHelper.EnableCache)
            {
                RealTimeCacheHelper.IncreaseListCacheVersion(entity);
                if (RealTimeCacheHelper.PropertyNameOfBody != null)
                {
                    string str = RealTimeCacheHelper.PropertyNameOfBody.GetValue(entity, null) as string;
                    await this.cacheService.AddAsync(RealTimeCacheHelper.GetCacheKeyOfEntityBody(entity.EntityId), str, RealTimeCacheHelper.CachingExpirationType);
                    RealTimeCacheHelper.PropertyNameOfBody.SetValue(entity, null, null);
                }
                await this.cacheService.AddAsync(RealTimeCacheHelper.GetCacheKeyOfEntity(entity.EntityId), entity, RealTimeCacheHelper.CachingExpirationType);
            }
        }

        /// <summary>
        /// 把实体entiy更新到数据库 
        /// </summary>
        /// <param name="entity"></param>
        public virtual async Task UpdateAsync(TEntity entity)
        {
            int num = await CreateDAO().UpdateAsync(entity);
            if (num > 0)
            {
                await this.OnUpdated(entity);
            }
        }

        /// <summary>
        /// 数据库更新实体后自动调用该方法 
        /// </summary>
        /// <param name="entity"></param>
        protected virtual async Task OnUpdated(TEntity entity)
        {
            if (RealTimeCacheHelper.EnableCache)
            {
                RealTimeCacheHelper.IncreaseEntityCacheVersion(entity.EntityId);
                RealTimeCacheHelper.IncreaseListCacheVersion(entity);
                if ((RealTimeCacheHelper.PropertyNameOfBody != null) && (RealTimeCacheHelper.PropertyNameOfBody.GetValue(entity, null) != null))
                {
                    string str = RealTimeCacheHelper.PropertyNameOfBody.GetValue(entity, null) as string;
                    await this.cacheService.SetAsync(RealTimeCacheHelper.GetCacheKeyOfEntityBody(entity.EntityId), str, RealTimeCacheHelper.CachingExpirationType);
                    RealTimeCacheHelper.PropertyNameOfBody.SetValue(entity, null, null);
                }
                await this.cacheService.SetAsync(RealTimeCacheHelper.GetCacheKeyOfEntity(entity.EntityId), entity, RealTimeCacheHelper.CachingExpirationType);
            }
        }

        /// <summary>
        /// 从数据库删除实体
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<int> DeleteAsync(TEntity entity)
        {
            if (entity == null)
            {
                return 0;
            }
            int num = 0;
            if (entity is IDelEntity)
            {
                (entity as IDelEntity).IsDeletedInDatabase = true;
                num = await CreateDAO().UpdateAsync(entity);
                if (num > 0)
                {
                    await this.OnDeleted(entity);
                }
            }
            else
            {
                num = await CreateDAO().DeleteAsync(entity);
                if (num > 0)
                {
                    await this.OnDeleted(entity);
                }
            }
            return num;
        }


        /// <summary>
        /// 数据库删除实体后自动调用该方法
        /// </summary>
        /// <param name="entity"></param>
        protected virtual async Task OnDeleted(TEntity entity)
        {
            if (RealTimeCacheHelper.EnableCache)
            {
                RealTimeCacheHelper.IncreaseEntityCacheVersion(entity.EntityId);
                RealTimeCacheHelper.IncreaseListCacheVersion(entity);
                await this.cacheService.MarkDeletionAsync(RealTimeCacheHelper.GetCacheKeyOfEntity(entity.EntityId), entity, CachingExpirationType.SingleObject);
            }
        }
        /// <summary>
        /// 从数据库删除实体(by EntityId)
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public virtual async Task<int> DeleteByEntityIdAsync(object entityId)
        {
            TEntity entity = await GetAsync(entityId);
            if (entity == null)
            {
                return 0;
            }
            return await DeleteAsync(entity);
        }

        #endregion

        #region Exists
        /// <summary>
        /// 依据主键检查实体是否存在于数据库 
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(object entityId)
        {
            return await CreateDAO().ExistsAsync<TEntity>(entityId);
        }
        #endregion

        /// <summary>
        /// 依据EntityId获取单个实体
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetAsync(object entityId)
        {
            TEntity local = default(TEntity);
            if (RealTimeCacheHelper.EnableCache)
            {
                local = await this.cacheService.GetAsync<TEntity>(RealTimeCacheHelper.GetCacheKeyOfEntity(entityId));
            }
            if (local == null)
            {
                var dao = CreateDAO();
                local = await dao.SingleOrDefaultAsync<TEntity>(entityId);
                if (RealTimeCacheHelper.EnableCache && (local != null))
                {
                    if (RealTimeCacheHelper.PropertyNameOfBody != null)
                    {
                        RealTimeCacheHelper.PropertyNameOfBody.SetValue(local, null, null);
                    }
                    await this.cacheService.AddAsync(RealTimeCacheHelper.GetCacheKeyOfEntity(local.EntityId), local, RealTimeCacheHelper.CachingExpirationType);
                }
            }
            if (local != null)
            {
                if (local is IDelEntity)
                {
                    if (!(local as IDelEntity).IsDeletedInDatabase)
                        return local;
                }
                else
                {
                    return local;
                }
            }
            return default(TEntity);
        }

        /// <summary>
        /// 获取所有实体（仅用于数据量少的情况）
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await GetAllAsync(null);
        }

        /// <summary>
        /// 获取所有实体（仅用于数据量少的情况） 
        /// </summary>
        /// <param name="orderBy">排序字段（多个字段用逗号分隔）</param>
        /// <returns>返回按照orderBy排序的所有实体集合</returns>
        public async Task<IEnumerable<TEntity>> GetAllAsync(string orderBy)
        {
            IEnumerable<object> enumerable = null;
            string cacheKey = null;
            if (RealTimeCacheHelper.EnableCache)
            {
                cacheKey = RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.GlobalVersion);
                if (!string.IsNullOrEmpty(orderBy))
                {
                    cacheKey = cacheKey + "SB-" + orderBy;
                }
                enumerable = await this.cacheService.GetAsync<IEnumerable<object>>(cacheKey);
            }
            if (enumerable == null)
            {
                PocoData data = PocoData.ForType(typeof(TEntity), new ConventionMapper());
                Sql sql = Sql.Builder.Select(new object[] { data.TableInfo.PrimaryKey }).From(new object[] { data.TableInfo.TableName });
                if (!string.IsNullOrEmpty(orderBy))
                {
                    sql.OrderBy(new object[] { orderBy });
                }
                enumerable = await CreateDAO().FetchFirstColumnAsync<object>(sql);
                if (RealTimeCacheHelper.EnableCache)
                {
                    await this.cacheService.AddAsync(cacheKey, enumerable, RealTimeCacheHelper.CachingExpirationType);
                }
            }
            return await PopulateEntitiesByEntityIdsAsync(enumerable);
        }

        /// <summary>
        /// 获取前topNumber条Entity（启用缓存） 
        /// </summary>
        /// <param name="topNumber"></param>
        /// <param name="cachingExpirationTypes">缓存策略</param>
        /// <param name="getCacheKey">生成cacheKey的委托</param>
        /// <param name="generateSql">生成PetaPoco.Sql的委托</param>
        /// <returns></returns>
        protected virtual async Task<IEnumerable<TEntity>> GetTopEntitiesAsync(int topNumber, CachingExpirationType cachingExpirationTypes, Func<string> getCacheKey, Func<Sql> generateSql)
        {

            PagingEntityIdCollection ids = null;
            string cacheKey = getCacheKey();
            ids = await this.cacheService.GetAsync<PagingEntityIdCollection>(cacheKey);
            if (ids == null)
            {
                ids = new PagingEntityIdCollection(await CreateDAO().FetchTopPrimaryKeysAsync<TEntity>(this.SecondaryMaxRecords, generateSql()));
                await this.cacheService.AddAsync(cacheKey, ids, cachingExpirationTypes);
            }
            IEnumerable<object> topEntityIds = ids.GetTopEntityIds(topNumber);
            return await PopulateEntitiesByEntityIdsAsync(topEntityIds);
        }

        /// <summary>
        /// 依据EntityId集合组装成实体集合（自动缓存） 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityIds"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TEntity>> PopulateEntitiesByEntityIdsAsync<T>(IEnumerable<T> entityIds)
        {
            TEntity[] localArray = new TEntity[entityIds.Count()];
            Dictionary<object, int> dictionary = new Dictionary<object, int>();
            for (int i = 0; i < entityIds.Count(); i++)
            {
                string cacheKey = RealTimeCacheHelper.GetCacheKeyOfEntity(entityIds.ElementAt(i));
                TEntity local = await this.cacheService.GetAsync<TEntity>(cacheKey);
                if (local != null)
                {
                    localArray[i] = local;
                }
                else
                {
                    localArray[i] = default(TEntity);
                    dictionary[entityIds.ElementAt(i)] = i;
                }
            }
            if (dictionary.Count > 0)
            {
                var dao = this.CreateDAO();
                await dao.OpenSharedConnectionAsync();
                IEnumerable<TEntity> entities;
                try
                {
                    entities = await dao.FetchByPrimaryKeysAsync<TEntity>(dictionary.Keys);
                }
                finally
                {
                    dao.CloseSharedConnection();
                }
                foreach (TEntity local in entities)
                {

                    if (!dictionary.ContainsKey(local.EntityId))
                        throw new Exception($"类型异常,dictionary:{entityIds.FirstOrDefault().GetType()}, EntityId:{local.EntityId.GetType()}");
                    var index = dictionary[local.EntityId];
                    localArray[index] = local;
                    if (RealTimeCacheHelper.EnableCache && (local != null))
                    {
                        if ((RealTimeCacheHelper.PropertyNameOfBody != null) && (RealTimeCacheHelper.PropertyNameOfBody != null))
                        {
                            RealTimeCacheHelper.PropertyNameOfBody.SetValue(local, null, null);
                        }
                        await this.cacheService.SetAsync(RealTimeCacheHelper.GetCacheKeyOfEntity(local.EntityId), local, RealTimeCacheHelper.CachingExpirationType);
                    }
                }
            }
            List<TEntity> list = new List<TEntity>();
            foreach (TEntity local in localArray)
            {
                if (local != null)
                {
                    if (local is IDelEntity)
                    {
                        if (!(local as IDelEntity).IsDeletedInDatabase)
                            list.Add(local);
                    }
                    else
                    {
                        list.Add(local);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// (默认)获取分页查询数据（直接读数据库无缓存）
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected virtual async Task<PagedList<TEntity>> GetDefPagingEntitiesAsync(int pageSize, int pageIndex, Sql sql)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Page<TEntity> page = await CreateDAO().PageAsync<TEntity>(pageIndex, pageSize, sql);
            stopwatch.Stop();
            TimeSpan timespan = stopwatch.Elapsed;
            return new PagedList<TEntity>(page.Items, pageIndex, pageSize, page.TotalItems) { Duration = timespan.TotalSeconds };
        }

        /// <summary>
        /// 获取分页查询数据 （主键不缓存，实体缓存）
        /// </summary>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">当前页码(从1开始)</param>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected virtual async Task<PagedList<TEntity>> GetPagingEntitiesAsync(int pageSize, int pageIndex, Sql sql)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            PagingEntityIdCollection ids = await CreateDAO().FetchPagingPrimaryKeysAsync<TEntity>(this.PrimaryMaxRecords, pageSize, pageIndex, sql);
            stopwatch.Stop();
            TimeSpan timespan = stopwatch.Elapsed;
            return new PagedList<TEntity>(await PopulateEntitiesByEntityIdsAsync(ids.GetPagingEntityIds(pageSize, pageIndex)), pageIndex, pageSize, ids.TotalRecords) { Duration = timespan.TotalSeconds };
        }

        /// <summary>
        /// 获取分页查询数据（主键和实体都启用缓存）
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="cachingExpirationTypes"></param>
        /// <param name="getCacheKey"></param>
        /// <param name="generateSql"></param>
        /// <returns></returns>
        protected virtual async Task<PagedList<TEntity>> GetPagingEntitiesAsync(int pageSize, int pageIndex, CachingExpirationType cachingExpirationTypes, Func<string> getCacheKey, Func<Sql> generateSql)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            PagingEntityIdCollection ids = null;
            if ((pageIndex < this.CacheablePageCount) && (pageSize <= this.SecondaryMaxRecords))
            {
                string cacheKey = getCacheKey.Invoke();
                ids = await this.cacheService.GetAsync<PagingEntityIdCollection>(cacheKey);
                if (ids == null)
                {
                    ids = await CreateDAO().FetchPagingPrimaryKeysAsync<TEntity>(this.PrimaryMaxRecords, pageSize * this.CacheablePageCount, 1, generateSql());
                    ids.IsContainsMultiplePages = true;
                    await this.cacheService.AddAsync(cacheKey, ids, cachingExpirationTypes);
                }
            }
            else
            {
                ids = await CreateDAO().FetchPagingPrimaryKeysAsync<TEntity>(this.PrimaryMaxRecords, pageSize, pageIndex, generateSql());
            }
            stopwatch.Stop();
            TimeSpan timespan = stopwatch.Elapsed;
            return new PagedList<TEntity>(await PopulateEntitiesByEntityIdsAsync(ids.GetPagingEntityIds(pageSize, pageIndex)), pageIndex, pageSize, ids.TotalRecords) { Duration = timespan.TotalSeconds };
        }

        /// <summary>
        /// 可缓存的列表缓存页数
        /// </summary>
        protected virtual int CacheablePageCount
        {
            get
            {
                return this.cacheablePageCount;
            }
        }

        /// <summary>
        /// 主流查询最大允许返回记录数 
        /// </summary>
        protected virtual long PrimaryMaxRecords
        {
            get
            {
                return this.primaryMaxRecords;
            }
        }
        /// <summary>
        /// 缓存实时性辅助类
        /// </summary>
        protected static RealTimeCacheHelper RealTimeCacheHelper
        {
            get
            {
                return EntityData.ForType(typeof(TEntity)).RealTimeCacheHelper;
            }
        }


        /// <summary>
        /// 非主流查询最大允许返回记录数
        /// </summary>
        protected virtual int SecondaryMaxRecords
        {
            get
            {
                return this.secondaryMaxRecords;
            }
        }
    }
}
