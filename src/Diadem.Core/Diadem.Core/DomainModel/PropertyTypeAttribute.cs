using System;

namespace Diadem.Core.DomainModel
{
    public class PropertyTypeAttribute : Attribute
    {
        public PropertyTypeAttribute()
        {
        }

        public PropertyTypeAttribute(PropertyType propertyType)
        {
            PropertyType = propertyType;
        }

        public PropertyType PropertyType { get; }
    }
}