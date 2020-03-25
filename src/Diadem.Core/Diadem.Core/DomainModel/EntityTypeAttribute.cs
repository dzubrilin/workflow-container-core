using System;

namespace Diadem.Core.DomainModel
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EntityTypeAttribute : Attribute
    {
        public EntityTypeAttribute(string entityType)
        {
            EntityType = entityType;
        }

        public string EntityType { get; set; }
    }
}