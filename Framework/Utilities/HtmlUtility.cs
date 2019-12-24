using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Linq;
using HtmlAgilityPack;

namespace PmSoft.Utilities
{
    public sealed class TrustedHtmlContainer
    {
        public TrustedHtml trustedHtml { get; set; }
    }

    public class HtmlUtility
    {

        /// <summary>
        /// 图片地址转换为绝对路径
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string ImageUrlAsAbsolute(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            string rootUrl = string.Empty;
            var httpContext = WebUtility.GetHttpContext();
            if (httpContext != null && httpContext.Request != null)
                rootUrl = WebUtility.HostPath(httpContext.Request) + WebUtility.ResolveUrl("~/uploads");
            return Regex.Replace(html, @"<img([^<]*)src=""(\/Uploads)(.*?)""([^<]*)>", "<img$1src=\"" + rootUrl + "$3\"$4>", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 移除Html标签
        /// </summary>
        /// <param name="rawString">待处理字符串</param>
        /// <param name="removeHtmlEntities">是否移除Html实体</param>
        /// <param name="enableMultiLine">是否保留换行符</param>
        /// <returns></returns>
        public static string StripHtml(string rawString, bool removeHtmlEntities, bool enableMultiLine)
        {
            if (string.IsNullOrWhiteSpace(rawString))
                return string.Empty;
            string input = rawString;
            if (enableMultiLine)
            {
                input = Regex.Replace(Regex.Replace(input, @"</p(?:\s*)>(?:\s*)<p(?:\s*)>", "\n\n", RegexOptions.Compiled | RegexOptions.IgnoreCase), @"<br(?:\s*)/>", "\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            input = input.Replace("\"", "''");
            if (removeHtmlEntities)
            {
                input = Regex.Replace(input, "&[^;]*;", string.Empty, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            return Regex.Replace(input, "<[^>]+>", string.Empty, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Html标签过滤/清除 
        /// </summary>
        /// <param name="rawHtml"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        //public static string CleanHtml(string rawHtml, TrustedHtmlLevel level)
        //{
        //    if (string.IsNullOrEmpty(rawHtml))
        //    {
        //        return rawHtml;
        //    }
        //    HtmlDocument document = new HtmlDocument
        //    {
        //        OptionAutoCloseOnEnd = true,
        //        OptionWriteEmptyNodes = true
        //    };
        //    TrustedHtml trustedHtml = ServiceLocator.GetService<TrustedHtml>();
        //    switch (level)
        //    {
        //        case TrustedHtmlLevel.Basic:
        //            trustedHtml = trustedHtml.Basic();
        //            break;

        //        case TrustedHtmlLevel.HtmlEditor:
        //            trustedHtml = trustedHtml.HtmlEditor();
        //            break;
        //    }
        //    document.LoadHtml(rawHtml);
        //    HtmlNodeCollection source = document.DocumentNode.SelectNodes("//*");
        //    if (source != null)
        //    {
        //        Dictionary<string, string> enforcedAttributes;
        //        string host = string.Empty;
        //        if (HttpContext.Current != null)
        //        {
        //            host = WebUtility.HostPath(HttpContext.Current.Request.Url);
        //        }
        //        source.ToList<HtmlNode>().ForEach(delegate(HtmlNode n)
        //        {
        //            Action<HtmlAttribute> action = null;

        //            if (trustedHtml.IsSafeTag(n.Name))
        //            {
        //                if (action == null)
        //                {
        //                    action = delegate(HtmlAttribute attr)
        //                    {
        //                        if (!trustedHtml.IsSafeAttribute(n.Name, attr.Name, attr.Value))
        //                        {
        //                            attr.Remove();
        //                        }
        //                        else if (attr.Value.StartsWith("javascirpt:", StringComparison.InvariantCultureIgnoreCase))
        //                        {
        //                            attr.Value = "javascirpt:;";
        //                        }
        //                    };
        //                }
        //                n.Attributes.ToList<HtmlAttribute>().ForEach(action);
        //                enforcedAttributes = trustedHtml.GetEnforcedAttributes(n.Name);
        //                if (enforcedAttributes != null)
        //                {
        //                    foreach (KeyValuePair<string, string> pair in enforcedAttributes)
        //                    {
        //                        if (!(from a in n.Attributes select a.Name).Contains<string>(pair.Key))
        //                        {
        //                            n.Attributes.Add(pair.Key, pair.Value);
        //                        }
        //                        else
        //                        {
        //                            n.Attributes[pair.Key].Value = pair.Value;
        //                        }
        //                    }
        //                }
        //                if ((n.Name == "a") && n.Attributes.Contains("href"))
        //                {
        //                    string str = n.Attributes["href"].Value;
        //                    if (str.StartsWith("http://") && !str.ToLowerInvariant().StartsWith(host.ToLower()))
        //                    {
        //                        if (!(from a in n.Attributes select a.Name).Contains<string>("rel"))
        //                        {
        //                            n.Attributes.Add("rel", "nofollow");
        //                        }
        //                        else if (n.Attributes["rel"].Value != "fancybox")
        //                        {
        //                            n.Attributes["rel"].Value = "nofollow";
        //                        }
        //                    }
        //                }
        //            }
        //            else if (trustedHtml.EncodeHtml)
        //            {
        //                //n.HtmlEncode = true;
        //            }
        //            else
        //            {
        //                //n.RemoveTag();
        //            }
        //        });
        //    }
        //    return document.DocumentNode.WriteTo();
        //}

        /// <summary>
        /// 闭合未闭合的Html标签 
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string CloseHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }
            HtmlDocument document = new HtmlDocument
            {
                OptionAutoCloseOnEnd = true,
                OptionWriteEmptyNodes = true
            };
            document.LoadHtml(html);
            return document.DocumentNode.WriteTo();
        }

        /// <summary>
        /// 清除UBB标签 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string StripBBTags(string content)
        {
            return Regex.Replace(content, @"\[[^\]]*?\]", string.Empty, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 移除Html用于内容预览 
        /// </summary>
        /// <param name="rawString"></param>
        /// <returns></returns>
        public static string StripForPreview(string rawString)
        {
            return StripBBTags(StripHtml(rawString.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n").Replace("<p>", "\n").Replace("'", "&#39;"), false, false));
        }


        /// <summary>
        /// 移除script标签
        /// </summary>
        /// <param name="rawString"></param>
        /// <returns></returns>
        public static string StripScriptTags(string rawString)
        {
            rawString = Regex.Replace(rawString, "<script((.|\n)*?)</script>", "", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            rawString = rawString.Replace("\"javascript:", "\"");
            return rawString;
        }

        /// <summary>
        /// 移除html内的Elemtnts/Attributes及 ，超过charLimit个字符进行截断
        /// </summary>
        /// <param name="rawHtml"></param>
        /// <param name="charLimit"></param>
        /// <returns></returns>
        public static string TrimHtml(string rawHtml, int charLimit)
        {
            if (string.IsNullOrEmpty(rawHtml))
            {
                return string.Empty;
            }
            string rawString = StripBBTags(StripHtml(rawHtml, true, false));
            if ((charLimit > 0) && (charLimit < rawString.Length))
            {
                return StringUtility.Trim(rawString, charLimit);
            }
            return rawString;
        }
    }
}
