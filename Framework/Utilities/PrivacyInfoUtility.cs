using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

namespace PmSoft.Utilities
{
    /// <summary>
    /// 隐私信息类型
    /// </summary>
    public enum PrivacyInfoType
    {
        /// <summary>
        /// 手机
        /// </summary>
        Mobile = 0,
        /// <summary>
        /// 身份证号码
        /// </summary>
        CardId = 1,
        /// <summary>
        /// 邮箱
        /// </summary>
        Email = 2,
        /// <summary>
        /// 姓名
        /// </summary>
        TrueName = 3
    }

    /// <summary>
    /// 隐私信息工具类
    /// </summary>
    public class PrivacyInfoUtility
    {
        /// <summary>
        /// 隐藏隐私信息
        /// </summary>
        /// <param name="input"></param>
        /// <param name="privacyType"></param>
        /// <returns></returns>
        public static string HidePrivacyInfo(string input, PrivacyInfoType privacyInfoType = PrivacyInfoType.Mobile)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            if (privacyInfoType == PrivacyInfoType.Mobile)
            {
                return Regex.Replace(input, "(\\d{3})\\d{4}(\\d{4})", "$1****$2");
            }
            else if (privacyInfoType == PrivacyInfoType.CardId)
            {
                if (input.Length <= 8) return input;
                return ReplaceWithSpecialChar(input);
            }
            else if (privacyInfoType == PrivacyInfoType.TrueName)
            {
                return HideName(input);
            }
            return input;
        }

        /// <summary>
        /// 将传入的字符串中间部分字符替换成特殊字符
        /// </summary>
        /// <param name="value">需要替换的字符串</param>
        /// <param name="startLen">前保留长度</param>
        /// <param name="endLen">尾保留长度</param>
        /// <param name="replaceChar">特殊字符</param>
        /// <returns>被特殊字符替换的字符串</returns>
        private static string ReplaceWithSpecialChar(string value, int startLen = 4, int endLen = 4, char specialChar = '*')
        {
            try
            {
                int lenth = value.Length - startLen - endLen;

                string replaceStr = value.Substring(startLen, lenth);

                string specialStr = string.Empty;

                for (int i = 0; i < replaceStr.Length; i++)
                {
                    specialStr += specialChar;
                }

                value = value.Replace(replaceStr, specialStr);
            }
            catch (Exception)
            {
                throw;
            }

            return value;
        }

        private static string HideName(string input)
        {
            string name = string.Empty;

            if (string.IsNullOrEmpty(input))
                return name;

            if (input.Length == 1)
                name = input;

            if (input.Length == 2)
                name = input.Replace(input.Substring(1), "*");

            if (input.Length > 2)
                name = ReplaceWithSpecialChar(input, 1, 1, '*');

            return name;
        }

    }
}