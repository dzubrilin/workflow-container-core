using System;
using System.Linq;
using System.Reflection;

namespace Diadem.Core.Common
{
    public static class TypeExtensions
    {
        public static bool TryGetPropertyInfo(this Type type, string name, out PropertyInfo propertyInfo)
        {
            propertyInfo = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            return null != propertyInfo;
        }

        public static bool TryGetPropertyInfo<T>(this Type type, string name, out PropertyInfo propertyInfo)
        {
            if (!type.TryGetPropertyInfo(name, out propertyInfo))
            {
                return false;
            }

            if (propertyInfo.PropertyType == typeof(T))
            {
                return true;
            }

            if (propertyInfo.PropertyType.IsGenericType &&
                propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                propertyInfo.PropertyType.GetGenericArguments().Single() == typeof(T))
            {
                return true;
            }

            throw new Exception($"Can not set property[{name}] of type [{propertyInfo.PropertyType}] with value of type [{typeof(T)}]");
        }
    }
}