using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PmSoft
{
    /// <summary>
    ///  时间格式化转换
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 获取时期天起始时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetDayBeginTime(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return DateTime.Now.Date;
            return GetDayBeginTime(dateTime.Value);
        }

        /// <summary>
        /// 获取时期天起始时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetDayBeginTime(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        /// 获取时期天的结束时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetDayEndTime(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return DateTime.Now;
            return GetDayEndTime(dateTime.Value);
        }

        /// <summary>
        /// 获取时期天的结束时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetDayEndTime(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59);
        }

        /// <summary>
        /// 与当前时间差值(分钟)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int WithDiffMinutes(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return 0;
            return WithDiffMinutes(dateTime.Value);
        }

        /// <summary>
        /// 与当前时间差值(分钟)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int WithDiffMinutes(this DateTime dateTime)
        {
            if (dateTime == null) return 0;
            TimeSpan ts1 = new TimeSpan(dateTime.Ticks);
            TimeSpan ts2 = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            return (int)ts.TotalMinutes;
        }

        /// <summary>
        /// 转换到剩余时间
        /// </summary>
        /// <param name="deadline"></param>
        /// <returns></returns>
        public static string ToTimeLeft(this DateTime? deadline)
        {
            if (!deadline.HasValue) return "N/A";
            return (deadline.Value).ToTimeLeft();
        }

        /// <summary>
        /// 转换到剩余时间
        /// </summary>
        /// <param name="deadline"></param>
        /// <returns></returns>
        public static string ToTimeLeft(this DateTime deadline)
        {
            if (deadline == null) return "N/A";
            int totalSeconds = (int)deadline.Subtract(DateTime.Now).TotalSeconds;
            TimeSpan ts = new TimeSpan(0, 0, totalSeconds);
            if (ts.Days > 0)
            {
                return ts.Days.ToString() + "天";
            }
            if (ts.Days == 0 && ts.Hours > 0)
            {
                return ts.Hours.ToString() + "小时 ";
            }
            if (ts.Days == 0 && ts.Hours == 0 && ts.Minutes > 0)
            {
                return ts.Minutes.ToString() + "分钟 ";
            }
            return "N/A";
        }

        /// <summary>
        /// 格式化为(月/日)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToMonthDayFormat(this DateTime? dateTime)
        {
            if (dateTime.HasValue)
                return dateTime.Value.ToMonthDayFormat();
            else
                return "N/A";
        }

        /// <summary>
        /// 格式化为(月/日)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToMonthDayFormat(this DateTime dateTime)
        {
            return dateTime.ToString("MM/dd");
        }

        /// <summary>
        /// 标准格式(yyyy-MM-dd hh:mm:ss)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToStandardFormat(this DateTime? dateTime)
        {
            if (dateTime.HasValue)
                return dateTime.Value.ToStandardFormat();
            else
                return "N/A";
        }


        /// <summary>
        /// 标准格式(yyyy-MM-dd hh:mm:ss)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToStandardFormat(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-M-dd HH:mm");
        }

        /// <summary>
        /// 转换为日期
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToDateFormat(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 智能时间提示(yyyy-MM-dd hh:mm 和 刚刚)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToSmartFormat(this DateTime? dateTime)
        {
            if (dateTime.HasValue)
                return dateTime.Value.ToSmartFormat();
            else
                return "N/A";
        }

        /// <summary>
        /// 智能时间提示(yyyy-MM-dd hh:mm 和 刚刚)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToSmartFormat(this DateTime dateTime)
        {
            string strTime = "";
            DateTime date1 = DateTime.Now;
            DateTime date2 = dateTime;
            TimeSpan dt = date1 - date2;

            // 相差天数
            int days = dt.Days;
            // 时间点相差小时数
            int hours = dt.Hours;
            // 相差总小时数
            double Minutes = dt.Minutes;
            // 相差总秒数
            int second = dt.Seconds;

            if (days == 0 && hours == 0 && Minutes == 0)
            {
                strTime = "刚刚";
            }
            else if (days == 0 && hours == 0)
            {
                strTime = Minutes + "分钟前";
            }
            else if (days == 0)
            {
                strTime = hours + "小时前";
            }
            else
            {
                strTime = dateTime.ToString("yyyy-M-dd HH:mm");
            }
            return strTime;
        }

        /// <summary>
        /// 转换为时期数字格式(例如:20180324)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int ToDateNumber(this DateTime dateTime)
        {
            return dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day;
        }
    }
}
