using System;
using System.Collections.Generic;
namespace PmSoft.Utilities
{
    /// <summary>
    /// 受信任Html标签的严格程度 
    /// </summary>
    public enum TrustedHtmlLevel
    {
        /// <summary>
        /// 普通受信任标签 允许使用个别html标签，例如：无Html编辑器的评论表单
        /// </summary>
        Basic,
        /// <summary>
        /// 针对于Html编辑器的受信任标签 
        /// </summary>
        HtmlEditor
    }

    /// <summary>
    /// Html标签过滤配置类
    /// </summary>
    public class TrustedHtml
    {
        private Dictionary<string, HashSet<string>> _attributes;
        private bool _encodeHtml;
        private Dictionary<string, Dictionary<string, string>> _enforcedAttributes;
        private HashSet<string> _globalAttributes;
        private Dictionary<string, Dictionary<string, HashSet<string>>> _protocols;
        private HashSet<string> _tagNames;
        protected static Dictionary<TrustedHtmlLevel, TrustedHtml> addedRules = new Dictionary<TrustedHtmlLevel, TrustedHtml>();

        public TrustedHtml()
            : this(false)
        {
        }

        public TrustedHtml(bool encodeHtml)
        {
            this._encodeHtml = encodeHtml;
            this._tagNames = new HashSet<string>();
            this._globalAttributes = new HashSet<string>();
            this._attributes = new Dictionary<string, HashSet<string>>();
            this._enforcedAttributes = new Dictionary<string, Dictionary<string, string>>();
            this._protocols = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        }

        /// <summary>
        /// 添加受信任的标签属性规则 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public TrustedHtml AddAttributes(string tag, params string[] keys)
        {
            if (string.IsNullOrEmpty(tag))
            {
                throw new ArgumentNullException("tag");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            HashSet<string> set = new HashSet<string>();
            foreach (string str in keys)
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw new Exception("key");
                }
                set.Add(str);
            }
            if (this._attributes.ContainsKey(tag))
            {
                foreach (string str2 in set)
                {
                    this._attributes[tag].Add(str2);
                }
            }
            else
            {
                this._attributes.Add(tag, set);
            }
            return this;
        }

        /// <summary>
        /// 添加必须存在的标签属性规则 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public TrustedHtml AddEnforcedAttribute(string tag, string key, string value)
        {
            if (string.IsNullOrEmpty(tag))
            {
                throw new ArgumentNullException("tag");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }
            if (this._enforcedAttributes.ContainsKey(tag))
            {
                this._enforcedAttributes[tag].Add(key, value);
            }
            else
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add(key, value);
                this._enforcedAttributes.Add(tag, dictionary);
            }
            return this;
        }

        /// <summary>
        /// 添加全局受信任的属性 
        /// </summary>
        /// <param name="attrs"></param>
        /// <returns></returns>
        public TrustedHtml AddGlobalAttributes(params string[] attrs)
        {
            if (attrs == null)
            {
                throw new ArgumentNullException("attributes");
            }
            foreach (string str in attrs)
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw new Exception("An empty attribute was found.");
                }
                this._globalAttributes.Add(str);
            }
            return this;
        }

        /// <summary>
        /// 添加标签中被允许协议的Url规则 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="key"></param>
        /// <param name="protocols"></param>
        /// <returns></returns>
        public TrustedHtml AddProtocols(string tag, string key, params string[] protocols)
        {
            Dictionary<string, HashSet<string>> dictionary;
            HashSet<string> set;
            if (string.IsNullOrEmpty(tag))
            {
                throw new ArgumentNullException("tag");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            if (protocols == null)
            {
                throw new ArgumentNullException("protocols");
            }
            if (this._protocols.ContainsKey(tag))
            {
                dictionary = this._protocols[tag];
            }
            else
            {
                dictionary = new Dictionary<string, HashSet<string>>();
                this._protocols.Add(tag, dictionary);
            }
            if (dictionary.ContainsKey(key))
            {
                set = dictionary[key];
            }
            else
            {
                set = new HashSet<string>();
                dictionary.Add(key, set);
            }
            foreach (string str in protocols)
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw new Exception("protocol is empty.");
                }
                set.Add(str);
            }
            return this;
        }

        /// <summary>
        /// 添加受信任的标签验证规则 
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public TrustedHtml AddTags(params string[] tags)
        {
            if (tags == null)
            {
                throw new ArgumentNullException("tags");
            }
            foreach (string str in tags)
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw new Exception("An empty tag was found.");
                }
                this._tagNames.Add(str);
            }
            return this;
        }

        /// <summary>
        /// 普通受信任标签
        /// </summary>
        /// <returns></returns>
        public virtual TrustedHtml Basic()
        {
            if (!addedRules.ContainsKey(TrustedHtmlLevel.Basic))
            {
                addedRules[TrustedHtmlLevel.Basic] = new TrustedHtml(this._encodeHtml).AddTags(new string[] { 
                    "strong", "em", "u", "b", "i", "font", "ul", "ol", "li", "p", "address", "div", "hr", "br", "a", "span", 
                    "img"
                 }).AddGlobalAttributes(new string[] { "align", "style" }).AddAttributes("font", new string[] { "size", "color", "face" }).AddAttributes("em", new string[] { "rel" }).AddAttributes("p", new string[] { "dir" }).AddAttributes("a", new string[] { "href", "title", "name", "target", "rel" }).AddAttributes("img", new string[] { "src", "alt", "title", "border", "width", "height" }).AddProtocols("a", "href", new string[] { "ftp", "http", "https", "mailto" });
            }
            return addedRules[TrustedHtmlLevel.Basic];
        }

        /// <summary>
        /// 获取强制添加的标签属性 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public virtual Dictionary<string, string> GetEnforcedAttributes(string tag)
        {
            if (this._enforcedAttributes.ContainsKey(tag))
            {
                return this._enforcedAttributes[tag];
            }
            return null;
        }

        /// <summary>
        /// 编辑器中受信任的标签
        /// </summary>
        /// <returns></returns>
        public virtual TrustedHtml HtmlEditor()
        {
            if (!addedRules.ContainsKey(TrustedHtmlLevel.HtmlEditor))
            {
                addedRules[TrustedHtmlLevel.HtmlEditor] = new TrustedHtml(this._encodeHtml).AddTags(new string[] { 
                    "h1", "h2", "h3", "h4", "h5", "h6", "h7", "strong", "em", "u", "b", "i", "strike", "sub", "sup", "font", 
                    "blockquote", "ul", "ol", "li", "p", "address", "div", "hr", "br", "a", "span", "img", "table", "tbody", "th", "td", 
                    "tr", "pre", "code", "xmp", "object", "param", "embed"
                 }).AddGlobalAttributes(new string[] { "align", "id", "style" }).AddAttributes("font", new string[] { "size", "color", "face" }).AddAttributes("blockquote", new string[] { "dir" }).AddAttributes("p", new string[] { "dir" }).AddAttributes("em", new string[] { "rel" }).AddAttributes("a", new string[] { "href", "title", "name", "target", "rel" }).AddAttributes("img", new string[] { "src", "alt", "title", "border", "width", "height" }).AddAttributes("table", new string[] { "border", "cellpadding", "cellspacing", "bgcorlor", "width" }).AddAttributes("th", new string[] { "bgcolor", "width" }).AddAttributes("td", new string[] { "rowspan", "colspan", "bgcolor", "width" }).AddAttributes("pre", new string[] { "name", "class" }).AddAttributes("object", new string[] { "classid", "codebase", "width", "height", "data", "type" }).AddAttributes("param", new string[] { "name", "value" }).AddAttributes("embed", new string[] { "type", "src", "width", "height", "quality", "scale", "bgcolor", "vspace", "hspace", "base", "flashvars", "swliveconnect" }).AddProtocols("a", "href", new string[] { "ftp", "http", "https", "mailto" }).AddProtocols("img", "src", new string[] { "http", "https" }).AddProtocols("blockquote", "cite", new string[] { "http", "https" }).AddProtocols("cite", "cite", new string[] { "http", "https" });
            }
            return addedRules[TrustedHtmlLevel.HtmlEditor];
        }

        /// <summary>
        /// 验证标签属性是否被信任 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="attr"></param>
        /// <param name="attrVal"></param>
        /// <returns></returns>
        public virtual bool IsSafeAttribute(string tag, string attr, string attrVal)
        {
            if (!this._globalAttributes.Contains(attr) && (!this._attributes.ContainsKey(tag) || !this._attributes[tag].Contains(attr)))
            {
                return false;
            }
            if (this._protocols.ContainsKey(tag) && this._protocols[tag].ContainsKey(attr))
            {
                return this.ValidProtocol(tag, attr, attrVal);
            }
            return true;
        }

        /// <summary>
        /// 判断标签是否被信任 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public virtual bool IsSafeTag(string tag)
        {
            return this._tagNames.Contains(tag);
        }

        /// <summary>
        /// 验证标签实行中的Url是否符合限制条件 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="attr"></param>
        /// <param name="attVal"></param>
        /// <returns></returns>
        private bool ValidProtocol(string tag, string attr, string attVal)
        {
            if (!attVal.ToLowerInvariant().Contains("javascript:"))
            {
                if (!attVal.Contains("://"))
                {
                    return true;
                }
                foreach (string str in this._protocols[tag][attr])
                {
                    string str2 = str + ":";
                    if (attVal.ToLowerInvariant().StartsWith(str2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 是否需要Html编码
        /// </summary>
        public bool EncodeHtml
        {
            get
            {
                return this._encodeHtml;
            }
        }
    }
}

