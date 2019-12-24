using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PmSoft.Utilities
{
    /// <summary>
    /// XML工具类
    /// </summary>
    public class XMLUtility
    {
        /// <summary>
        /// 将自定义对象序列化为XML字符串
        /// </summary>
        /// <param name="obj">自定义对象实体</param>
        /// <returns>序列化后的XML字符串</returns>
        public static string SerializeToXml<T>(T obj)
        {
            if (obj != null)
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));

                using (MemoryStream stream = new MemoryStream())
                {
                    using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
                    {
                        writer.Formatting = Formatting.None;//缩进
                        xs.Serialize(writer, obj);

                        stream.Position = 0;
                        StringBuilder sb = new StringBuilder();
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                sb.Append(line);
                            }
                            reader.Close();
                        }

                        return sb.ToString();
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 将XML字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="xml">XML字符</param>
        /// <returns></returns>
        public static T DeserializeToObject<T>(string xml)
        {
            T result;
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                result = (T)serializer.Deserialize(reader);
            }
            return result;
        }
    }
}
