using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PmSoft
{
    /// <summary>
    /// 枚举类扩展
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="value">枚举</param>
        public static string GetDescription(this Enum value)
        {
            if (value == null)
                return "N/A";
            string name = Enum.GetName(value.GetType(), value);
            if (string.IsNullOrEmpty(name))
                return value.ToString();
            var attribute = value.GetType().GetField(name).GetCustomAttributes(
                 typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false)
                 .Cast<System.ComponentModel.DataAnnotations.DisplayAttribute>()
                 .FirstOrDefault();
            if (attribute != null)
                return attribute.Name;

            return value.ToString();
        }
    }
}
