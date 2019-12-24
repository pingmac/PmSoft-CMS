using System;
using System.Collections.Generic;
using PmSoft.Utilities;
namespace PmSoft
{
    /// <summary>
    /// IDictionary扩展
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// 依据key获取字典的value，并转换为需要的类型 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary">字典集合</param>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">如果未找到则返回该默认值</param>
        /// <returns></returns>
        public static T Get<T>(this IDictionary<string, object> dictionary, string key, T defaultValue)
        {
            if (dictionary.ContainsKey(key))
            {
                object obj;
                if (dictionary.TryGetValue(key, out obj))
                    return ValueUtility.ChangeType<T>(obj, defaultValue);
            }
            return defaultValue;
        }

        /// <summary>
        /// 解析为以逗号分隔的数组字符表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetList<T>(this IDictionary<string, object> dictionary, string key, IEnumerable<T> defaultValue)
        {
            if (dictionary.ContainsKey(key))
            {
                object obj;
                if (dictionary.TryGetValue(key, out obj) && obj != null)
                    return ValueUtility.ConvertToList<T>(obj.ToString()).ToArray();
            }
            return defaultValue;
        }
    }
}
