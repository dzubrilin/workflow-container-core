using System.Collections.Generic;

namespace Diadem.Core.DomainModel
{
    public sealed class DynamicExtendableEntity : ExtendableEntity, IExtendableEntityConfiguration
    {
        public DynamicExtendableEntity(string type)
        {
            EntityType = type;
        }

        public override string EntityType { get; }

        void IExtendableEntityConfiguration.SetDynamicProperty<T>(DynamicProperty<T> property)
        {
            Properties[property.Name] = property;
        }

        void IExtendableEntityConfiguration.SetDynamicEntity(string name, DynamicExtendableEntity entity)
        {
            Entities[name] = entity;
        }

        public override T GetProperty<T>(string name)
        {
            return GetDynamicProperty<T>(name);
        }

        public override void SetProperty<T>(string name, T value)
        {
            SetDynamicProperty(name, value);
        }

        public override IEntity GetEntity(string name)
        {
            return GetDynamicEntity(name);
        }

        public override void SetCollection(string name, IList<IEntity> collection)
        {
            Collections[name] = collection;
        }

        public override IList<IEntity> GetCollection(string name)
        {
            return GetDynamicCollection(name);
        }
    }
}