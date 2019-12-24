using System.Collections.Generic;
using PmSoft;

namespace PetaPoco
{
    /// <summary>
    /// 扩展的同步方法
    /// </summary>
    public interface IExtend
    {
        /// <summary>
        /// 是否繁忙中
        /// </summary>
        bool IsBustling { get; }

        /// <summary>
        /// 执行多个SQL
        /// </summary>
        /// <param name="sqls"></param>
        /// <returns></returns>
        int Execute(IEnumerable<Sql> sqls);

        /// <summary>
        /// 查询主键集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKeys"></param>
        /// <returns></returns>
        IEnumerable<T> FetchByPrimaryKeys<T>(IEnumerable<object> primaryKeys);

        /// <summary>
        /// 获取第一列组成的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        IEnumerable<T> FetchFirstColumn<T>(Sql sql);

        /// <summary>
        /// 获取第一列组成的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        IEnumerable<T> FetchFirstColumn<T>(string sql, params object[] args);

        /// <summary>
        /// 获取前topNumber条记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topNumber"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        IEnumerable<T> FetchTop<T>(int topNumber, Sql sql);

        /// <summary>
        /// 获取前topNumber条记录
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="topNumber"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        IEnumerable<object> FetchTopPrimaryKeys<TEntity>(int topNumber, Sql sql) where TEntity : IEntity;

        /// <summary>
        /// 取可分页的主键集合 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="maxRecords"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        PagingEntityIdCollection FetchPagingPrimaryKeys<TEntity>(long maxRecords, int pageSize, int pageIndex, Sql sql) where TEntity : IEntity;

        /// <summary>
        /// 获取可分页的主键集合 
        /// </summary>
        /// <param name="maxRecords"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="primaryKey"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        PagingEntityIdCollection FetchPagingPrimaryKeys(long maxRecords, int pageSize, int pageIndex, string primaryKey, Sql sql);
    }
}
