using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PmSoft
{
    /// <summary>
    /// IEnumerable 只读集合扩展方法
    /// </summary>
    public static class ReadOnlyCollectionExtensions
    {
        /// <summary>
        /// 转换成只读集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">可枚举的集合</param>
        /// <returns>返回只读集合</returns>
        public static IList<T> ToReadOnly<T>(this IEnumerable<T> enumerable)
        {
            return new ReadOnlyCollection<T>(enumerable.ToList<T>());
        }
    }
}
