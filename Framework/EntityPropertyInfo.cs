using System;
using System.Reflection;

namespace PmSoft
{
    [Serializable]
    public class EntityPropertyInfo
    {
        public EntityPropertyInfo() { }

        internal EntityPropertyInfo(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; set; }

        public object GetValue(object obj, object[] index)
        {
            Type type = obj.GetType();
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance;
            PropertyInfo propertyInfo = type.GetProperty(Name, bindingFlags);
            return propertyInfo.GetValue(obj);
        }

        public void SetValue(object obj, object value, object[] index)
        {
            Type type = obj.GetType();
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance;
            PropertyInfo propertyInfo = type.GetProperty(Name, bindingFlags);
            propertyInfo.SetValue(obj, value, index);
        }
    }

    public static class PropertyInfoExtensions
    {
        public static EntityPropertyInfo AsEntityPropertyInfo(this PropertyInfo propertyInfo)
        {
            return new EntityPropertyInfo(propertyInfo.Name);
        }
    }
}
