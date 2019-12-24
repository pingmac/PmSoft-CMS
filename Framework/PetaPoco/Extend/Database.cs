using PetaPoco.Core;
using PetaPoco.Internal;
using PetaPoco.Providers;
using PetaPoco.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PmSoft;

namespace PetaPoco
{
    public partial class Database : IExtendAsync, IExtend
    {

        public bool IsBustling { get { return _connectionLock.CurrentCount == 0; } }


        public int Execute(IEnumerable<Sql> sqls)
        {
            return ExecuteAsync(sqls).Result;
        }

        public async Task<int> ExecuteAsync(IEnumerable<Sql> sqls)
        {
            int num2;
            try
            {
                await OpenSharedConnectionAsync();
                try
                {
                    await _connectionLock.WaitAsync().ConfigureAwait(false);
                    int num = 0;
                    foreach (Sql sql in sqls)
                    {
                        using (IDbCommand command = this.CreateCommand(this.Connection, sql.SQL, sql.Arguments))
                        {
                            num += command.ExecuteNonQuery();
                            OnExecutedCommand(command);
                            continue;
                        }
                    }
                    num2 = num;
                }
                finally
                {
                    _connectionLock.Release();
                    CloseSharedConnection();
                }
            }
            catch (Exception exception)
            {
                this.OnException(exception);
                throw;
            }
            return num2;
        }


        public IEnumerable<T> FetchByPrimaryKeys<T>(IEnumerable<object> primaryKeys)
        {
            return FetchByPrimaryKeysAsync<T>(primaryKeys).Result;
        }


        public async Task<IEnumerable<T>> FetchByPrimaryKeysAsync<T>(IEnumerable<object> primaryKeys)
        {
            if ((primaryKeys == null) || (primaryKeys.Count<object>() == 0))
            {
                return new List<T>();
            }

            IEnumerable<object> selectKeys;
            IEnumerable<object> nextKeys = null;
            if (primaryKeys.Count() > 1000)
            {
                selectKeys = primaryKeys.Take(1000);
                nextKeys = primaryKeys.Except(selectKeys);
            }
            else
                selectKeys = primaryKeys;

            string str = _provider.EscapeSqlIdentifier(PocoData.ForType(typeof(T), DefaultMapper).TableInfo.PrimaryKey);
            StringBuilder builder = new StringBuilder("WHERE ");
            int num = 0;
            using (IEnumerator<object> enumerator = selectKeys.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    object current = enumerator.Current;
                    builder.AppendFormat("{0} = @{1} or ", str, num);
                    num++;
                }
            }
            builder.Remove(builder.Length - 4, 3);

            /*解决IN 超过2100个参数出错问题 实现分次加载 */
            if (nextKeys != null)
            {
                List<T> results = await FetchAsync<T>(builder.ToString(), selectKeys.ToArray<object>());
                results.AddRange(await FetchByPrimaryKeysAsync<T>(nextKeys));
                return results;
            }
            else
                return await FetchAsync<T>(builder.ToString(), selectKeys.ToArray<object>());

        }

        /// <summary>
        /// 获取第一列组成的集合
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public IEnumerable<T> FetchFirstColumn<T>(Sql sql)
        {
            return FetchFirstColumnAsync<T>(sql).Result;
        }

        /// <summary>
        /// 获取第一列组成的集合
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> FetchFirstColumnAsync<T>(Sql sql)
        {
            return await FetchFirstColumnAsync<T>(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 获取第一列组成的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IEnumerable<T> FetchFirstColumn<T>(string sql, params object[] args)
        {
            return FetchFirstColumnAsync<T>(sql, args).Result;
        }

        /// <summary>
        /// 获取第一列组成的集合
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> FetchFirstColumnAsync<T>(string sql, params object[] args)
        {
            List<T> list = new List<T>();
            try
            {
                await OpenSharedConnectionAsync();
                await _connectionLock.WaitAsync().ConfigureAwait(false);
                using (IDbCommand command = CreateCommand(this.Connection, sql, args))
                {
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        this.OnExecutedCommand(command);
                        while (reader.Read())
                        {
                            if (reader[0] is T)
                                list.Add((T)reader[0]);
                        }
                        reader.Close();
                    }
                    //Type type = list.FirstOrDefault().GetType();
                    return list;
                }
            }
            finally
            {
                _connectionLock.Release();
                this.CloseSharedConnection();
            }
        }


        /// <summary>
        /// 构建获取前topNumber记录的SQL 
        /// </summary>
        /// <param name="topNumber"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected string BuildTopSql(int topNumber, string sql)
        {
            Match match = ((PagingHelper)Provider.PagingUtility).RegexColumns.Match(sql);
            if (!match.Success)
            {
                return null;
            }
            Group group = match.Groups[1];
            if (this._provider is MySqlDatabaseProvider)
            {
                return string.Concat(new object[] { sql.Substring(0, group.Index), " ", group.Value, " ", sql.Substring(group.Index + group.Length), " limit ", topNumber });
            }
            return string.Concat(new object[] { sql.Substring(0, group.Index), " top ", topNumber, " ", group.Value, " ", sql.Substring(group.Index + group.Length) });
        }

        /// <summary>
        /// 构建获取前topNumber记录的SQL 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topNumber"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected string BuildTopSql<T>(int topNumber, string sql)
        {
            PocoData data = PocoData.ForType(typeof(T), DefaultMapper);
            string primaryKey = data.TableInfo.TableName + "." + data.TableInfo.PrimaryKey;
            if (this.EnableAutoSelect)
            {
                sql = AutoSelectHelperExt.AddSelectClause<T>(Provider, sql, DefaultMapper, primaryKey);
            }
            return this.BuildTopSql(topNumber, sql);
        }

        /// <summary>
        /// 创建分页的SQL语句 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="maxRecords"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <param name="sqlCount"></param>
        /// <param name="sqlPage"></param>
        protected void BuildPagingPrimaryKeyQueries<T>(long maxRecords, long skip, long take, string sql, ref object[] args, out string sqlCount, out string sqlPage)
        {
            PocoData data = PocoData.ForType(typeof(T), DefaultMapper);
            string primaryKey = string.Empty;
            if (sql.Contains(data.TableInfo.TableName))
            {
                primaryKey = data.TableInfo.TableName + "." + data.TableInfo.PrimaryKey;
            }
            else
            {
                primaryKey = data.TableInfo.PrimaryKey;
            }
            if (this.EnableAutoSelect)
            {
                sql = AutoSelectHelperExt.AddSelectClause<T>(Provider, sql, DefaultMapper, primaryKey);
            }
            this.BuildPagingPrimaryKeyQueries(maxRecords, skip, take, primaryKey, sql, ref args, out sqlCount, out sqlPage);
        }

        /// <summary>
        /// 创建分页的SQL语句 
        /// </summary>
        /// <param name="maxRecords"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="primaryKey"></param>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <param name="sqlCount"></param>
        /// <param name="sqlPage"></param>
        protected void BuildPagingPrimaryKeyQueries(long maxRecords, long skip, long take, string primaryKey, string sql, ref object[] args, out string sqlCount, out string sqlPage)
        {
            string str;
            string str2;
            if (!SplitSqlForPagingOptimized(maxRecords, sql, primaryKey, out sqlCount, out str, out str2))
            {
                throw new Exception("Unable to parse SQL statement for paged query");
            }
            SQLParts parts;
            parts.Sql = sql;
            parts.SqlCount = sqlCount;
            parts.SqlSelectRemoved = str;
            parts.SqlOrderBy = str2;
            sqlPage = Provider.BuildPageQuery(skip, take, parts, ref args, primaryKey);
        }

        /// <summary>
        /// 获取可分页的主键集合 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="maxRecords"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public PagingEntityIdCollection FetchPagingPrimaryKeys<TEntity>(long maxRecords, int pageSize, int pageIndex, Sql sql) where TEntity : IEntity
        {
            return FetchPagingPrimaryKeysAsync<TEntity>(maxRecords, pageSize, pageIndex, sql).Result;
        }

        /// <summary>
        /// 获取可分页的主键集合 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="maxRecords"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public async Task<PagingEntityIdCollection> FetchPagingPrimaryKeysAsync<TEntity>(long maxRecords, int pageSize, int pageIndex, Sql sql) where TEntity : IEntity
        {
            if (pageIndex < 1)
                pageIndex = 1;
            string str2;
            string str3;
            string sQL = sql.SQL;
            object[] arguments = sql.Arguments;
            this.BuildPagingPrimaryKeyQueries<TEntity>(maxRecords, (pageIndex - 1) * pageSize, pageSize, sQL, ref arguments, out str2, out str3);
            return new PagingEntityIdCollection(
                await FetchFirstColumnAsync<object>(str3, arguments),
                await ExecuteScalarAsync<int>(str2, arguments));
        }

        /// <summary>
        /// 获取可分页的主键集合 
        /// </summary>
        /// <param name="maxRecords"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="primaryKey"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public PagingEntityIdCollection FetchPagingPrimaryKeys(long maxRecords, int pageSize, int pageIndex, string primaryKey, Sql sql)
        {
            return FetchPagingPrimaryKeysAsync(maxRecords, pageSize, pageIndex, primaryKey, sql).Result;
        }

        /// <summary>
        /// 获取可分页的主键集合 
        /// </summary>
        /// <param name="maxRecords"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="primaryKey"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public async Task<PagingEntityIdCollection> FetchPagingPrimaryKeysAsync(long maxRecords, int pageSize, int pageIndex, string primaryKey, Sql sql)
        {
            string str2;
            string str3;
            string sQL = sql.SQL;
            object[] arguments = sql.Arguments;
            this.BuildPagingPrimaryKeyQueries(maxRecords, (pageIndex - 1) * pageSize, pageSize, primaryKey, sQL, ref arguments, out str2, out str3);
            return new PagingEntityIdCollection(await FetchFirstColumnAsync<object>(str3, arguments), await ExecuteScalarAsync<int>(str2, arguments));
        }

        /// <summary>
        /// 获取前topNumber条记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topNumber"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public IEnumerable<T> FetchTop<T>(int topNumber, Sql sql)
        {
            return FetchTopAsync<T>(topNumber, sql).Result;
        }


        /// <summary>
        /// 获取前topNumber条记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topNumber"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> FetchTopAsync<T>(int topNumber, Sql sql)
        {
            string sQL = sql.SQL;
            object[] arguments = sql.Arguments;
            string str2 = this.BuildTopSql(topNumber, sQL);
            return await FetchAsync<T>(str2, arguments);
        }

        /// <summary>
        /// 获取前topNumber条记录
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="topNumber"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public IEnumerable<object> FetchTopPrimaryKeys<TEntity>(int topNumber, Sql sql) where TEntity : IEntity
        {
            return FetchTopPrimaryKeysAsync<TEntity>(topNumber, sql).Result;
        }

        /// <summary>
        /// 获取前topNumber条记录
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="topNumber"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public async Task<IEnumerable<object>> FetchTopPrimaryKeysAsync<TEntity>(int topNumber, Sql sql) where TEntity : IEntity
        {
            string sQL = sql.SQL;
            object[] arguments = sql.Arguments;
            string str2 = this.BuildTopSql<TEntity>(topNumber, sQL);
            return await FetchFirstColumnAsync<object>(str2, arguments);
        }



        protected bool SplitSqlForPagingOptimized(long maxRecords, string sql, string primaryKey, out string sqlCount, out string sqlSelectRemoved, out string sqlOrderBy)
        {
            sqlSelectRemoved = null;
            sqlCount = null;
            sqlOrderBy = null;
            Match match = ((PagingHelper)Provider.PagingUtility).RegexColumns.Match(sql);
            if (!match.Success)
            {
                return false;
            }
            Group group = match.Groups[1];
            sqlSelectRemoved = sql.Substring(group.Index);
            if (((PagingHelper)Provider.PagingUtility).RegexDistinct.IsMatch(sqlSelectRemoved))
            {
                sqlCount = sql.Substring(0, group.Index) + "COUNT(" + match.Groups[1].ToString().Trim() + ") " + sql.Substring(group.Index + group.Length);
            }
            else if (maxRecords > 0L)
            {
                if (this.Provider is MySqlDatabaseProvider)
                {
                    sqlCount = string.Concat(new object[] { "select count(*) from (", sql, " limit ", maxRecords, " ) as TempCountTable" });
                }
                else
                {
                    sqlCount = string.Concat(new object[] { "select count(*) from (", sql.Substring(0, group.Index), " top ", maxRecords, " ", primaryKey, " ", sql.Substring(group.Index + group.Length), " ) as TempCountTable" });
                }
            }
            else
            {
                sqlCount = sql.Substring(0, group.Index) + "COUNT(*) " + sql.Substring(group.Index + group.Length);
            }
            match = rxOrderBy.Match(sqlCount);
            if (!match.Success)
            {
                sqlOrderBy = null;
            }
            else
            {
                group = match.Groups[0];
                sqlOrderBy = group.ToString();
                sqlCount = sqlCount.Substring(0, group.Index) + sqlCount.Substring(group.Index + group.Length);
            }
            return true;
        }

        Regex rxOrderBy = new Regex(
       @"\bORDER\s+BY\s+(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*",
       RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
    }
}
