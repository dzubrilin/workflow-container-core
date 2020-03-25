using System;
using System.Reflection;

namespace Diadem.Core.DomainModel
{
    public static class DomainModelExtensions
    {
        public static string GetEntityTypeName(this Type type)
        {
            var entityTypeAttribute = type.GetCustomAttribute<EntityTypeAttribute>();
            return null != entityTypeAttribute ? entityTypeAttribute.EntityType : type.FullName;
        }
    }
}