using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PmSoft.Utilities
{
    /// <summary>
    /// 权重对象
    /// </summary>
    public interface IWeightObject
    {
        /// <summary>
        /// 权重
        /// </summary>
        int Weight { get; }
    }

    public class WeightObject<T> : IWeightObject
    {
        public WeightObject(T obj, int Weight)
        {
            this.Object = obj;
            this.Weight = Weight;
        }

        public int Weight { get; }

        public T Object { get; set; }
    }

    public static class Rand
    {
        public static char[] Pattern = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        public static Color[] PatternColor = new Color[] { Color.Red, Color.Black, Color.Blue, Color.Green, Color.Orange, Color.Brown, Color.DarkBlue };

        /// <summary>
        /// 按权重随机
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        static public T WeightObject<T>(IList<T> list) where T : IWeightObject
        {
            T result = default(T);
            int sum = 0;
            for (int i = 0; i < list.Count; i++)
                sum += list[i].Weight;
            for (int i = 0; i < list.Count; i++)
            {
                int randomNumber = new Random(Guid.NewGuid().GetHashCode()).Next(sum);
                if (randomNumber < list[i].Weight && list[i].Weight > 0)
                {
                    result = list[i];
                    break;
                }
                else
                    sum -= list[i].Weight;
            }
            if (result == null)
            {
                result = list.OrderByDescending(m => m.Weight).FirstOrDefault();
            }
            return result;
        }

        static public T WeightObject<T>(IList<T> list, int sum) where T : IWeightObject
        {
            T result = default(T);
            for (int i = 0; i < list.Count; i++)
            {
                int randomNumber = new Random(Guid.NewGuid().GetHashCode()).Next(sum);
                if (randomNumber < list[i].Weight && list[i].Weight > 0)
                {
                    result = list[i];
                    break;
                }
                else
                {
                    sum -= list[i].Weight;
                }
            }
            return result;
        }

        /// <summary>
        /// 根据GUID生成长整型
        /// </summary>
        /// <returns></returns>
        public static long GuidToLongID()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }

        /// <summary>
        /// 取布尔比例
        /// </summary>
        /// <param name="num"></param>
        /// <param name="maxNum"></param>
        /// <returns></returns>
        static public bool Boolean(int num, int maxNum = 100)
        {
            if (num >= maxNum)
                num = maxNum - 1;
            int result = new Random(Guid.NewGuid().GetHashCode()).Next(0, maxNum);
            if (result <= num)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 随机布尔
        /// </summary>
        /// <returns></returns>
        static public bool Boolean()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            return Next(0, 2) > 0;
        }

        static public int Next()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            return random.Next();
        }

        static public int Next(int MaxValue)
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            return random.Next(MaxValue);
        }

        static public int Next(int MinValue, int MaxValue)
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            if (MinValue >= MaxValue)
                MaxValue = MinValue + 1;
            return random.Next(MinValue, MaxValue);
        }


        /// <summary>
        /// 产生序号
        /// </summary>
        public static string GetSerialNumber(string startChar = "")
        {
            Random rand = new Random();
            return startChar + DateTime.Now.ToString("yyyyMMdd") + rand.Next(1000, 9999);
        }

        /// <summary>
        /// 生成随机数字
        /// </summary>
        /// <param name="length">生成长度</param>
        /// <returns></returns>
        public static string Number(int Length)
        {
            return Number(Length, false);
        }

        /// <summary>
        /// 生成随机数字
        /// </summary>
        /// <param name="Length">生成长度</param>
        /// <param name="Sleep">是否要在生成前将当前线程阻止以避免重复</param>
        /// <returns></returns>
        public static string Number(int Length, bool Sleep)
        {
            if (Sleep)
                System.Threading.Thread.Sleep(3);
            string result = "";
            System.Random random = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < Length; i++)
            {
                result += random.Next(10).ToString();
            }
            return result;
        }

        /// <summary>
        /// 生成随机字母与数字
        /// </summary>
        /// <param name="IntStr">生成长度</param>
        /// <returns></returns>
        public static string Str(int Length)
        {
            return Str(Length, false);
        }
        /// <summary>
        /// 生成随机字母与数字
        /// </summary>
        /// <param name="Length">生成长度</param>
        /// <param name="Sleep">是否要在生成前将当前线程阻止以避免重复</param>
        /// <returns></returns>
        public static string Str(int Length, bool Sleep)
        {
            if (Sleep)
                System.Threading.Thread.Sleep(3);
            string result = "";
            int n = Pattern.Length;
            System.Random random = new Random(~unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < Length; i++)
            {
                int rnd = random.Next(0, n);
                result += Pattern[rnd];
            }
            return result;
        }


        /// <summary>
        /// 生成随机纯字母随机数
        /// </summary>
        /// <param name="IntStr">生成长度</param>
        /// <returns></returns>
        public static string Str_char(int Length)
        {
            return Str_char(Length, false);
        }

        /// <summary>
        /// 生成随机纯字母随机数
        /// </summary>
        /// <param name="Length">生成长度</param>
        /// <param name="Sleep">是否要在生成前将当前线程阻止以避免重复</param>
        /// <returns></returns>
        public static string Str_char(int Length, bool Sleep)
        {
            if (Sleep)
                System.Threading.Thread.Sleep(3);
            string result = "";
            int n = Pattern.Length;
            System.Random random = new Random(~unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < Length; i++)
            {
                int rnd = random.Next(10, n);
                result += Pattern[rnd];
            }
            return result;
        }

        /// <summary>
        /// 得到随机颜色代码
        /// </summary>
        /// <returns></returns>
        public static Color Str_Color(bool Sleep)
        {
            if (Sleep)
            {
                System.Threading.Thread.Sleep(3);
            }
            int n = PatternColor.Length;
            System.Random random = new Random(~unchecked((int)DateTime.Now.Ticks));
            int rnd = random.Next(0, n);
            return PatternColor[rnd];
            //return result;
        }
        /// <summary>
        /// 生成随机数字
        /// </summary>
        /// <param name="length">生成长度</param>
        /// <returns></returns>
        public static byte[] Bytes(int Length)
        {
            return Bytes(Length, false);
        }

        /// <summary>
        /// 生成随机数字
        /// </summary>
        /// <param name="Length">生成长度</param>
        /// <param name="Sleep">是否要在生成前将当前线程阻止以避免重复</param>
        /// <returns></returns>
        public static byte[] Bytes(int Length, bool Sleep)
        {
            if (Sleep)
                System.Threading.Thread.Sleep(3);
            byte[] b = new byte[Length];
            System.Random random = new Random();
            random.NextBytes(b);
            return b;
        }

        /// <summary>
        /// 生产随机中文
        /// </summary>
        /// <param name="strlength"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string Str_Chs(int strlength, Encoding encode)
        {
            string[] rBase = new String[16] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f" };//定义一个字符串数组储存汉字编码的组成元素
            Random rnd = new Random();
            string strs = string.Empty;
            for (int i = 0; i < strlength; i++)
            {
                //区位码第1位
                int r1 = rnd.Next(11, 14);
                string str_r1 = rBase[r1].Trim();
                //区位码第2位
                rnd = new Random(r1 * unchecked((int)DateTime.Now.Ticks) + i);//更换随机数发生器的种子避免产生重复值
                int r2;
                if (r1 == 13)
                { r2 = rnd.Next(0, 7); }
                else
                { r2 = rnd.Next(0, 16); }
                string str_r2 = rBase[r2].Trim();
                //区位码第3位
                rnd = new Random(r2 * unchecked((int)DateTime.Now.Ticks) + i);
                int r3 = rnd.Next(10, 16);
                string str_r3 = rBase[r3].Trim();
                //区位码第4位
                rnd = new Random(r3 * unchecked((int)DateTime.Now.Ticks) + i);
                int r4;
                if (r3 == 10)
                { r4 = rnd.Next(1, 16); }
                else if (r3 == 15)
                { r4 = rnd.Next(0, 15); }
                else
                { r4 = rnd.Next(0, 16); }
                string str_r4 = rBase[r4].Trim();
                //定义两个字节变量存储产生的随机汉字区位码
                byte byte1 = Convert.ToByte(str_r1 + str_r2, 16);
                byte byte2 = Convert.ToByte(str_r3 + str_r4, 16);
                byte[] str_r = new byte[] { byte1, byte2 };//将两个字节变量存储在字节数组中
                strs += encode.GetString((byte[])Convert.ChangeType(str_r, typeof(byte[])));
            }
            return strs;
        }
    }
}
