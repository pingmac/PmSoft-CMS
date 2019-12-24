using System;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PmSoft.Utilities
{
    /// <summary>
    /// 字符串工具类
    /// </summary>
    public static class StringUtility
    {
        /// <summary>
        /// 取中文文本的拼音
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static string GetPinyin(string ch, bool removeSpace = false)
        {
            string pinyin = Pinyin.Pinyin.GetPinyin(ch);
            if (!string.IsNullOrEmpty(pinyin))
                pinyin = Regex.Replace(pinyin, @"\s", string.Empty);
            return pinyin;
        }

        /// <summary>
        /// 获取拼音首字母
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static string GetPinyinInitials(string ch)
        {
            return Pinyin.Pinyin.GetInitials(ch);
        }

        /// <summary>
        /// 清除XML无效字符
        /// </summary>
        /// <param name="rawXml"></param>
        /// <returns></returns>
        public static string CleanInvalidCharsForXML(string rawXml)
        {
            if (string.IsNullOrEmpty(rawXml))
            {
                return rawXml;
            }
            StringBuilder builder = new StringBuilder();
            char[] chArray = rawXml.ToCharArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                int num2 = Convert.ToInt32(chArray[i]);
                if ((((num2 < 0) || (num2 > 8)) && ((num2 < 11) || (num2 > 12))) && ((num2 < 14) || (num2 > 0x1f)))
                {
                    builder.Append(chArray[i]);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// 移除SQL注入代码
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static string StripSQLInjection(string sql)
        {
            if (!string.IsNullOrEmpty(sql))
            {
                string pattern = @"((\%27)|(\'))\s*((\%6F)|o|(\%4F))((\%72)|r|(\%52))";
                string str2 = @"(\%27)|(\')|(\-\-)";
                string str3 = @"\s+exec(\s|\+)+(s|x)p\w+";
                sql = Regex.Replace(sql, pattern, string.Empty, RegexOptions.IgnoreCase);
                sql = Regex.Replace(sql, str2, string.Empty, RegexOptions.IgnoreCase);
                sql = Regex.Replace(sql, str3, string.Empty, RegexOptions.IgnoreCase);
                sql = sql.Replace("%", "[%]");
            }
            return sql;
        }

        public static string Trim(string rawString, int charLimit)
        {
            return Trim(rawString, charLimit, "...");
        }

        public static string Trim(string rawString, int charLimit, string appendString)
        {
            if (string.IsNullOrEmpty(rawString) || (rawString.Length <= charLimit))
            {
                return rawString;
            }
            if (Encoding.UTF8.GetBytes(rawString).Length <= (charLimit * 2))
            {
                return rawString;
            }
            charLimit = (charLimit * 2) - Encoding.UTF8.GetBytes(appendString).Length;
            StringBuilder builder = new StringBuilder();
            int num2 = 0;
            for (int i = 0; i < rawString.Length; i++)
            {
                char ch = rawString[i];
                builder.Append(ch);
                num2 += (ch > '\x0080') ? 2 : 1;
                if (num2 >= charLimit)
                {
                    break;
                }
            }
            return builder.Append(appendString).ToString();
        }

        /// <summary>
        /// Unicode编码
        /// </summary>
        /// <param name="rawString"></param>
        /// <returns></returns>
        public static string UnicodeEncode(string rawString)
        {
            if ((rawString == null) || (rawString == string.Empty))
            {
                return rawString;
            }
            StringBuilder builder = new StringBuilder();
            string str2 = rawString;
            for (int i = 0; i < str2.Length; i++)
            {
                int num = str2[i];
                string str = "";
                if (num > 0x7e)
                {
                    builder.Append(@"\u");
                    str = num.ToString("x");
                    for (int j = 0; j < (4 - str.Length); j++)
                    {
                        builder.Append("0");
                    }
                }
                else
                {
                    str = ((char)num).ToString();
                }
                builder.Append(str);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Unicode解码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UnicodeDecode(this string str)
        {
            //最直接的方法Regex.Unescape(str);
            StringBuilder strResult = new StringBuilder();
            if (!string.IsNullOrEmpty(str))
            {
                string[] strlist = str.Replace("\\", "").Split('u');
                try
                {
                    for (int i = 1; i < strlist.Length; i++)
                    {
                        int charCode = Convert.ToInt32(strlist[i], 16);
                        strResult.Append((char)charCode);
                    }
                }
                catch (FormatException)
                {
                    return Regex.Unescape(str);
                }
            }
            return strResult.ToString();
        }

        /// <summary>
        /// 获取枚举属性DisplayAttribute的NAME值
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescription<TEnum>(TEnum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi == null) return value.ToString();
            var attribute = fi.GetCustomAttributes(
                  typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false)
                   .Cast<System.ComponentModel.DataAnnotations.DisplayAttribute>()
                   .FirstOrDefault();
            if (attribute != null)
                return attribute.Name;
            return value.ToString();
        }


    }
}

