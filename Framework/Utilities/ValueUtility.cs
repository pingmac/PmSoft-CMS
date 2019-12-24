using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Data.SqlTypes;

namespace PmSoft.Utilities
{
    /// <summary>
    /// 用于类型转换的工具类
    /// </summary>
    public static class ValueUtility
    {

        /// <summary>
        /// 字符串转换为指定类型数组,字符串以“,”号拆分
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static List<T> ConvertToList<T>(string value)
        {
            if (string.IsNullOrEmpty(value)) return new List<T>();
            string[] values = value.Split(',');
            List<T> results = new List<T>();
            foreach (string val in values)
                if (!string.IsNullOrWhiteSpace(val))
                {
                    try
                    {
                        results.Add((T)Convert.ChangeType(val, typeof(T)));
                    }
                    catch { }
                }
            return results;
        }

        /// <summary>
        /// 把value转换成类型为T的数据，无法进行转换时返回defaultValue 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ChangeType<T>(object value)
        {
            return ChangeType<T>(value, default(T));
        }

        /// <summary>
        /// 把value转换成类型为T的数据，无法进行转换时返回defaultValue
        /// </summary>
        /// <typeparam name="T">需转换的类型</typeparam>
        /// <param name="value">待转换的数据</param>
        /// <param name="defalutValue">无法转换时需返回的默认值</param>
        /// <returns></returns>
        public static T ChangeType<T>(object value, T defalutValue)
        {
            if (value == null)
            {
                return defalutValue;
            }
            Type nullableType = typeof(T);
            if (nullableType.IsInterface || (nullableType.IsClass && (nullableType != typeof(string))))
            {
                if (value is T)
                {
                    return (T)value;
                }
                return defalutValue;
            }
            if (nullableType.IsGenericType && (nullableType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                return (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(nullableType));
            }
            if (nullableType.IsEnum)
            {
                return (T)Enum.Parse(nullableType, value.ToString());
            }
            return (T)Convert.ChangeType(value, nullableType);
        }

        public static object ChangeType(Type nullableType, object value, object defalutValue = null)
        {
            if (value == null)
            {
                return defalutValue;
            }
            if (nullableType.IsInterface || (nullableType.IsClass && (nullableType != typeof(string))))
            {
                if (value is string)
                {
                    return value;
                }
                return defalutValue;
            }
            if (nullableType.IsGenericType && (nullableType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                if (string.IsNullOrEmpty(value.ToString()))
                    return defalutValue;
                return Convert.ChangeType(value, Nullable.GetUnderlyingType(nullableType));
            }
            if (nullableType.IsEnum)
            {
                return Enum.Parse(nullableType, value.ToString());
            }
            return Convert.ChangeType(value, nullableType);
        }


        /// <summary>
        /// 获取安全的SQL Server DateTime 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime GetSafeSqlDateTime(DateTime date)
        {
            if (date < ((DateTime)SqlDateTime.MinValue))
            {
                return SqlDateTime.MinValue.Value.AddYears(1);
            }
            if (date > ((DateTime)SqlDateTime.MaxValue))
            {
                return (DateTime)SqlDateTime.MaxValue;
            }
            return date;
        }

        /// <summary>
        /// 获取安全的SQL Server int 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static int GetSafeSqlInt(int i)
        {
            if (i <= ((int)SqlInt32.MinValue))
            {
                return (((int)SqlInt32.MinValue) + 1);
            }
            if (i >= ((int)SqlInt32.MaxValue))
            {
                return (((int)SqlInt32.MaxValue) - 1);
            }
            return i;
        }

        /// <summary>
        /// 获取在SQL Server中可以使用的整型最大值
        /// </summary>
        /// <returns></returns>
        public static int GetSqlMaxInt()
        {
            return (((int)SqlInt32.MaxValue) - 1);
        }

        /// <summary>
        /// 把字符串数组转换成整型列表 
        /// </summary>
        /// <param name="strArray">需要转换的字符串数组</param>
        /// <returns></returns>
        public static List<int> ParseInt(string[] strArray)
        {
            List<int> list = new List<int>();
            if ((strArray != null) && (strArray.Length != 0))
            {
                foreach (string str in strArray)
                {
                    int result = 0;
                    if (int.TryParse(str, out result))
                    {
                        list.Add(result);
                    }
                }
            }
            return list;
        }
    }
}
