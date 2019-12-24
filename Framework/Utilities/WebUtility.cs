using System;
using System.Linq;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace PmSoft.Utilities
{
    /// <summary>
    /// 提供与Web请求时可使用的工具类，包括Url解析、Url/Html编码、获取IP地址、返回http状态码
    /// </summary>
    public static class WebUtility
    {
        public static readonly string HtmlNewLine = "<br />";

        /// <summary>
        /// 将IPv4格式的字符串转换为int型表示
        /// </summary>
        /// <param name="strIPAddress"></param>
        /// <returns></returns>
        public static uint IPToNumber(string strIPAddress)
        {
            IPAddress ip;
            if (!string.IsNullOrEmpty(strIPAddress)
                && IPAddress.TryParse(strIPAddress, out ip))
            {
                byte[] bytes = ip.GetAddressBytes();
                Array.Reverse(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
            return 0;
        }

        /// <summary>
        /// 整数转换为IP地址
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string NumberToIPStr(uint number)
        {
            uint netInt = (uint)IPAddress.HostToNetworkOrder((Int32)number);
            IPAddress ipaddr = new IPAddress(netInt);
            return ipaddr.ToString();
        }

        /// <summary>
        /// 获取数字化IP
        /// </summary>
        /// <returns></returns>
        public static uint GetIPNumber()
        {
            return IPToNumber(GetIP());
        }
        /// <summary>
        /// 获取IP地址 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetIP()
        {
            IHttpContextAccessor contextAccessor = ServiceLocator.GetService<IHttpContextAccessor>();
            if (contextAccessor == null || contextAccessor.HttpContext == null)
                return string.Empty;
            var userHostAddress = contextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(userHostAddress))
            {
                userHostAddress = contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            if (!Regex.IsMatch(userHostAddress, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$"))
            {
                return string.Empty;
            }
            return userHostAddress;

        }


        /// <summary>
        /// 获取根域名 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="domainRules">域名规则  ".com.cn", ".edu.cn", ".net.cn",</param>
        /// <returns>例如：http://www.google.co.uk，应该返回google.co.uk，http://www.test.googlepages.com,应该返回googlepages.com</returns>
        public static string GetServerDomain(Uri uri, string[] domainRules)
        {
            if (uri == null)
            {
                return string.Empty;
            }
            string str = uri.Host.ToString().ToLower();
            if (str.IndexOf('.') <= 0)
            {
                return str;
            }
            string[] strArray = str.Split(new char[] { '.' });
            string s = strArray.GetValue((int)(strArray.Length - 1)).ToString();
            int result = -1;
            if (int.TryParse(s, out result))
            {
                return str;
            }
            string oldValue = string.Empty;
            string str4 = string.Empty;
            string str5 = string.Empty;
            for (int i = 0; i < domainRules.Length; i++)
            {
                if (str.EndsWith(domainRules[i].ToLower()))
                {
                    oldValue = domainRules[i].ToLower();
                    str4 = str.Replace(oldValue, "");
                    if (str4.IndexOf('.') > 0)
                    {
                        string[] strArray2 = str4.Split(new char[] { '.' });
                        return (strArray2.GetValue((int)(strArray2.Length - 1)).ToString() + oldValue);
                    }
                    return (str4 + oldValue);
                }
                str5 = str;
            }
            return str5;
        }

        /// <summary>
        /// 获取带传输协议的完整的主机地址 
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static string HostPath(Microsoft.AspNetCore.Http.HttpRequest httpRequest)
        {
            if (httpRequest == null)
            {
                return string.Empty;
            }
            return (httpRequest.Scheme + Uri.SchemeDelimiter + httpRequest.Host.Value);
        }

        /// <summary>
        /// 获取基础URL
        /// </summary>
        /// <returns></returns>
        public static string GetBaseUrl()
        {
            IHttpContextAccessor contextAccessor = ServiceLocator.GetService<IHttpContextAccessor>();
            if (contextAccessor == null)
                throw new ArgumentNullException(nameof(contextAccessor));
            if (contextAccessor == null || contextAccessor.HttpContext == null)
                return string.Empty;
            return contextAccessor.HttpContext.Request.PathBase.Value;
        }

        /// <summary>
        /// 获取IUrlHelper
        /// </summary>
        /// <returns></returns>
        public static IUrlHelper GetUrlHelper()
        {
            IActionContextAccessor actionContextAccessor = ServiceLocator.GetService<IActionContextAccessor>();
            if (actionContextAccessor == null)
                throw new ArgumentNullException(nameof(actionContextAccessor));
            IUrlHelperFactory urlHelperFactory = ServiceLocator.GetService<IUrlHelperFactory>();
            if (urlHelperFactory == null)
                throw new ArgumentNullException(nameof(actionContextAccessor));
            return urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
        }

        /// <summary>
        /// html解码 
        /// </summary>
        /// <param name="rawContent"></param>
        /// <returns></returns>
        public static string HtmlDecode(string rawContent)
        {
            if (!string.IsNullOrEmpty(rawContent))
            {
                return HttpUtility.HtmlDecode(rawContent);
            }
            return rawContent;
        }

        /// <summary>
        /// html编码 
        /// </summary>
        /// <param name="rawContent"></param>
        /// <returns></returns>
        public static string HtmlEncode(string rawContent)
        {
            if (!string.IsNullOrEmpty(rawContent))
            {
                return HttpUtility.HtmlEncode(rawContent);
            }
            return rawContent;
        }


        public static string GetWebRootPath(string filePath)
        {
            if ((filePath.IndexOf(@":\") != -1) || (filePath.IndexOf(@"\\") != -1))
            {
                return filePath;
            }
            IHostingEnvironment hostingEnvironment = ServiceLocator.GetService<IHostingEnvironment>();

            filePath = filePath.Replace('/', Path.DirectorySeparatorChar).Replace("~", "");

            return Path.Combine(hostingEnvironment.WebRootPath, filePath);
        }

        public static string GetContentRootPath(string filePath)
        {
            IHostingEnvironment hostingEnvironment = ServiceLocator.GetService<IHostingEnvironment>();
            filePath = filePath.Replace('\\', '/').Replace("~", "");
            string path = hostingEnvironment.ContentRootPath;
            var dirs = filePath.Split('/');
            foreach (var d in dirs)
                if (!string.IsNullOrEmpty(d))
                    path = Path.Combine(path, d);
            return path;
        }


        /// <summary>
        /// 将URL转换为在请求客户端可用的 URL（转换 ~/ 为绝对路径） 
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        public static string ResolveUrl(string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl))
            {
                return relativeUrl;
            }
            else if (relativeUrl[0] == '~')
            {
                var segment = new PathString(relativeUrl.Substring(1));
                var applicationPath = GetHttpContext().Request.PathBase;

                return applicationPath.Add(segment).Value;
            }
            return relativeUrl;
        }

        /// <summary>
        /// 获取AspNetCore的HttpContext
        /// </summary>
        /// <returns></returns>
        public static HttpContext GetHttpContext()
        {
            IHttpContextAccessor httpContextAccessor = ServiceLocator.GetService<IHttpContextAccessor>();

            if (httpContextAccessor == null)
                throw new ArgumentNullException(nameof(IHttpContextAccessor));

            return httpContextAccessor.HttpContext;
        }



        /// <summary>
        /// Url解码 
        /// </summary>
        /// <param name="urlToDecode"></param>
        /// <returns></returns>
        public static string UrlDecode(string urlToDecode)
        {
            if (!string.IsNullOrEmpty(urlToDecode))
            {
                return HttpUtility.UrlDecode(urlToDecode);
            }
            return urlToDecode;
        }

        /// <summary>
        /// Url编码 
        /// </summary>
        /// <param name="urlToEncode"></param>
        /// <returns></returns>
        public static string UrlEncode(string urlToEncode)
        {
            if (string.IsNullOrEmpty(urlToEncode))
            {
                return urlToEncode;
            }
            return HttpUtility.UrlEncode(urlToEncode).Replace("'", "%27");
        }

        public static string TextToHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            return input.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace("\r", "<br/>");
        }

    }
}
