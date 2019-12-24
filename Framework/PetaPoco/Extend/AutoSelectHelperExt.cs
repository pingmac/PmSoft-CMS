using System.Linq;
using System.Text.RegularExpressions;
using PetaPoco.Core;

namespace PetaPoco
{
    static class AutoSelectHelperExt
    {
        public static string AddSelectClause<T>(IProvider provider, string sql, IMapper defaultMapper, string primaryKey = null)
        {
            if (sql.StartsWith(";"))
                return sql.Substring(1);

            if (!rxSelect.IsMatch(sql))
            {
                var pd = PocoData.ForType(typeof(T), defaultMapper);
                var tableName = provider.EscapeTableName(pd.TableInfo.TableName);
                string cols = string.IsNullOrEmpty(primaryKey) ? (pd.Columns.Count != 0 ? string.Join(", ", (from c in pd.QueryColumns select tableName + "." + provider.EscapeSqlIdentifier(c)).ToArray()) : "NULL")
                    : primaryKey;
                if (!rxFrom.IsMatch(sql))
                    sql = string.Format("SELECT {0} FROM {1} {2}", cols, tableName, sql);
                else
                    sql = string.Format("SELECT {0} {1}", cols, sql);
            }
            return sql;
        }

        static Regex rxSelect = new Regex(@"\A\s*(SELECT|EXECUTE|CALL)\s", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex rxFrom = new Regex(@"\A\s*FROM\s", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    }
}
