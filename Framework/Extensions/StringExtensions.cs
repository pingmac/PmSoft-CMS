using System;
using System.Collections.Generic;
using System.IO;
using PmSoft.Utilities;

namespace PmSoft
{
    /// <summary>
    /// string扩展方法
    /// </summary>
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string instance)
        {
            return string.IsNullOrWhiteSpace(instance);
        }

        public static bool IsNotNullAndWhiteSpace(this string instance)
        {
            return !string.IsNullOrWhiteSpace(instance);
        }


        public static bool IsNullOrEmpty(this string theString)
        {
            return string.IsNullOrEmpty(theString);
        }

        public static string ToFilePath(this string path)
        {
            return Path.Combine(path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
