using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PetaPoco.Core;
using PetaPoco.Utilities;

namespace PetaPoco.Providers
{
    public class SqlServerDatabaseProvider : DatabaseProvider
    {
        public override DbProviderFactory GetFactory()
            => GetFactory("System.Data.SqlClient.SqlClientFactory, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

        public override string BuildPageQuery(long skip, long take, SQLParts parts, ref object[] args, string primaryKey = "")
        {
            var helper = (PagingHelper)PagingUtility;
            // when the query does not contain an "order by", it is very slow
            if (helper.SimpleRegexOrderBy.IsMatch(parts.SqlSelectRemoved))
            {
                var m = helper.SimpleRegexOrderBy.Match(parts.SqlSelectRemoved);
                if (m.Success)
                {
                    var g = m.Groups[0];
                    parts.SqlSelectRemoved = parts.SqlSelectRemoved.Substring(0, g.Index);
                }
            }

            if (helper.RegexDistinct.IsMatch(parts.SqlSelectRemoved))
                parts.SqlSelectRemoved = "peta_inner.* FROM (SELECT " + parts.SqlSelectRemoved + ") peta_inner";

            //var sqlPage =
            //    $"SELECT * FROM (SELECT ROW_NUMBER() OVER ({parts.SqlOrderBy ?? "ORDER BY (SELECT NULL)"}) peta_rn, {parts.SqlSelectRemoved}) peta_paged WHERE peta_rn > @{args.Length} AND peta_rn <= @{args.Length + 1}";

            primaryKey = primaryKey ?? string.Empty;
            if (primaryKey.Contains<char>('.') && !primaryKey.EndsWith("."))
                primaryKey = primaryKey.Substring(primaryKey.LastIndexOf(".") + 1);//移除表名

            var sqlPage = string.IsNullOrEmpty(primaryKey) ? string.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER ({0}) peta_rn, {1}) peta_paged WHERE peta_rn>@{2} AND peta_rn<=@{3}", parts.SqlOrderBy == null ? "ORDER BY (SELECT NULL)" : parts.SqlOrderBy, parts.SqlSelectRemoved, args.Length, args.Length + 1)
                : string.Format("SELECT peta_paged.{4} FROM (SELECT ROW_NUMBER() OVER ({0}) peta_rn, {1}) peta_paged WHERE peta_rn>@{2} AND peta_rn<=@{3}",
                                    parts.SqlOrderBy == null ? "ORDER BY (SELECT NULL)" : parts.SqlOrderBy, parts.SqlSelectRemoved, args.Length, args.Length + 1, primaryKey);

            args = args.Concat(new object[] { skip, skip + take }).ToArray();
            return sqlPage;
        }

        public override object ExecuteInsert(Database db, IDbCommand cmd, string primaryKeyName)
            => ExecuteScalarHelper(db, cmd);

        public override string GetExistsSql()
            => "IF EXISTS (SELECT 1 FROM {0} WHERE {1}) SELECT 1 ELSE SELECT 0";

        public override string GetInsertOutputClause(string primaryKeyName)
            => $" OUTPUT INSERTED.[{primaryKeyName}]";

#if ASYNC
        public override Task<object> ExecuteInsertAsync(CancellationToken cancellationToken, Database db, IDbCommand cmd, string primaryKeyName)
            => ExecuteScalarHelperAsync(cancellationToken, db, cmd);
#endif
    }
}