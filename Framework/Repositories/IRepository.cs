using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PmSoft.Repositories
{
    /// <summary>
    /// 用于处理Entity持久化操作 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TEntity> where TEntity : class, IEntity
    {
        /// <summary>
        /// 把实体entity添加到数据库
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<object> InsertAsync(TEntity entity);
        /// <summary>
        /// 把实体entiy更新到数据库 
        /// </summary>
        /// <param name="entity"></param>
        Task UpdateAsync(TEntity entity);
        /// <summary>
        /// 从数据库删除实体
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(TEntity entity);
        /// <summary>
        /// 从数据库删除实体(by EntityId)
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        Task<int> DeleteByEntityIdAsync(object primaryKey);
        /// <summary>
        /// 依据主键检查实体是否存在于数据库 
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(object primaryKey);
        /// <summary>
        /// 依据EntityId获取单个实体
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        Task<TEntity> GetAsync(object primaryKey);
        /// <summary>
        /// 获取所有实体（仅用于数据量少的情况）
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAllAsync();
        /// <summary>
        /// 获取所有实体（仅用于数据量少的情况） 
        /// </summary>
        /// <param name="orderBy">排序字段（多个字段用逗号分隔）</param>
        /// <returns>返回按照orderBy排序的所有实体集合</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(string orderBy);
        /// <summary>
        /// 依据EntityId集合组装成实体集合（自动缓存） 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityIds"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> PopulateEntitiesByEntityIdsAsync<T>(IEnumerable<T> entityIds);

    }
}
