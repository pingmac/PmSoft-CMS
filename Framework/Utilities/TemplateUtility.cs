using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PmSoft.Utilities
{
    /// <summary>
    /// 模板工具
    /// </summary>
    public class TemplateUtility
    {
        /// <summary>
        /// 占位符替换: ${TagName} -> TagValue
        /// 例："${year}-${month}-${day}" -> "2018-11-28"
        /// </summary>
        /// <param name="template">模板字符串</param>
        /// <returns>替换后的字符串</returns>
        public static string ReplacePlaceholder<TValue>(string template, SafeMap<TValue> pairs)
        {
            string result = new string(template.ToCharArray());

            // TODO 需要优化为，匹配到 \$\{(\S*?)\} 后按照匹配到的内容作为Key在Map中寻找Value替换
            foreach (var key in pairs.Keys)
            {
                Regex regex = new Regex(@"\$\{" + key + @"\}");
                result = regex.Replace(result, pairs[key] != null ? pairs[key].ToString() : "");
            }
            return result;
        }
    }

    /// <summary>
    /// 安全的映射表，不存在的Key返回默认的Value，string类型将返回string.Empty（""）
    /// </summary>
    public class SafeMap<TValue>
    {
        /// <summary>
        /// 获取Keys
        /// </summary>
        public IEnumerable<string> Keys
        {
            get
            {
                return kvs.Keys;
            }
        }

        public Dictionary<string, TValue> Kvs
        {
            get
            {
                return kvs;
            }
        }

        private Dictionary<string, TValue> kvs = new Dictionary<string, TValue>();

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>值</returns>
        public TValue this[string index]
        {
            get
            {
                return Get(index);
            }
            set
            {
                Set(index, value);
            }
        }

        /// <summary>
        /// 获取Key对应的值
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>值</returns>
        public TValue Get(string index)
        {
            if (kvs.ContainsKey(index))
            {
                return kvs[index];
            }
            else
            {
                return default(TValue);
            }
        }
        /// <summary>
        /// 设置Key对应的值
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="value">值</param>
        public void Set(string index, TValue value)
        {
            kvs[index] = value;
        }

    }
}
