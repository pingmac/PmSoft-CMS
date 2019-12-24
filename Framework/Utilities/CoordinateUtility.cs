using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PmSoft.Utilities
{
    public class CoordinateRange
    {
        /// <summary>
        /// 最小维度
        /// </summary>
        public double MinLat { get; set; }
        /// <summary>
        /// 最大维度
        /// </summary>
        public double MaxLat { get; set; }
        /// <summary>
        /// 最小经度
        /// </summary>
        public double MinLng { get; set; }
        /// <summary>
        /// 最大经度
        /// </summary>
        public double MaxLng { get; set; }
    }

    /// <summary>
    /// 坐标工具类
    /// </summary>
    public class CoordinateUtility
    {
        private const double x_pi = 3.14159265358979324 * 3000.0 / 180.0;
        /// <summary>
        /// 中国正常坐标系GCJ02协议的坐标，转到 百度地图对应的 BD09 协议坐标
        /// </summary>
        /// <param name="lat">维度</param>
        /// <param name="lng">经度</param>
        public static void ConvertGCJ02ToBD09(ref double lat, ref double lng)
        {
            double x = lng, y = lat;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * x_pi);
            lng = z * Math.Cos(theta) + 0.0065;
            lat = z * Math.Sin(theta) + 0.006;
        }

        /// <summary>
        /// 百度地图对应的 BD09 协议坐标，转到 中国正常坐标系GCJ02协议的坐标
        /// </summary>
        /// <param name="lat">维度</param>
        /// <param name="lng">经度</param>
        public static void ConvertBD09ToGCJ02(ref double lat, ref double lng)
        {
            double x = lng - 0.0065, y = lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
            lng = z * Math.Cos(theta);
            lat = z * Math.Sin(theta);
        }

        /// <summary>
        /// 根据一个给定经纬度的点和距离，进行附近地点查询
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <param name="distance">距离（单位：公里或千米）</param>
        /// <returns>返回一个范围的4个点，最小纬度和纬度，最大经度和纬度</returns>
        public static CoordinateRange GetRange(double longitude, double latitude, double distance)
        {
            //先计算查询点的经纬度范围  
            double r = 6378.137;//地球半径千米  
            double dis = distance;//千米距离    
            double dlng = 2 * Math.Asin(Math.Sin(dis / (2 * r)) / Math.Cos(latitude * Math.PI / 180));
            dlng = dlng * 180 / Math.PI;//角度转为弧度  
            double dlat = dis / r;
            dlat = dlat * 180 / Math.PI;
            return new CoordinateRange { MinLat = latitude - dlat, MaxLat = latitude + dlat, MinLng = longitude - dlng, MaxLng = longitude + dlng };
        }

        /// <summary>
        /// 计算两点位置的距离，返回两点的距离，单位：公里或千米
        /// 该公式为GOOGLE提供，误差小于0.2米
        /// </summary>
        /// <param name="lat1">第一点纬度</param>
        /// <param name="lng1">第一点经度</param>
        /// <param name="lat2">第二点纬度</param>
        /// <param name="lng2">第二点经度</param>
        /// <returns>返回两点的距离，单位：公里或千米</returns>
        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            //地球半径，单位米
            double EARTH_RADIUS = 6378137;
            double radLat1 = Rad(lat1);
            double radLng1 = Rad(lng1);
            double radLat2 = Rad(lat2);
            double radLng2 = Rad(lng2);
            double a = radLat1 - radLat2;
            double b = radLng1 - radLng2;
            double result = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2))) * EARTH_RADIUS;
            return result / 1000;
        }

        /// <summary>
        /// 经纬度转化成弧度
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private static double Rad(double d)
        {
            return (double)d * Math.PI / 180d;
        }
    }
}