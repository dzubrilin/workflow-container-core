using System;
using System.Collections.Generic;
using System.Linq;
using Diadem.Core.Common;

namespace Diadem.Core.DomainModel
{
    public abstract class ExtendableEntity : IExtendableEntity, IExtendableEntityConfiguration
    {
        protected ExtendableEntity()
        {
            Collections = new Dictionary<string, IList<IEntity>>();
            Entities = new Dictionary<string, DynamicExtendableEntity>();
            Properties = new Dictionary<string, DynamicProperty>();
        }

        protected Dictionary<string, IList<IEntity>> Collections { get; }

        protected Dictionary<string, DynamicExtendableEntity> Entities { get; }

        protected Dictionary<string, DynamicProperty> Properties { get; }

        public virtual string EntityId
        {
            get => GetProperty<string>("EntityId");
            set => SetProperty("EntityId", value);
        }

        public virtual string EntityType => GetType().GetEntityTypeName();

        public virtual IList<IEntity> GetCollection(string name)
        {
            if (!GetType().TryGetPropertyInfo(name, out var propertyInfo))
            {
                return GetDynamicCollection(name);
            }

            var value = propertyInfo.GetValue(this);
            if (null == value)
            {
                return null;
            }

            if (propertyInfo.PropertyType.IsGenericType &&
                propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IList<>) &&
                typeof(ExtendableEntity).IsAssignableFrom(propertyInfo.PropertyType.GetGenericArguments().Single()))
            {
                if (!(value is IList<IEntity> list))
                {
                    throw new Exception($"Property [{name}] is not an entity collection");
                }

                return list;
            }

            return GetDynamicCollection(name);
        }

        public virtual void SetCollection(string name, IList<IEntity> collection)
        {
            if (!GetType().TryGetPropertyInfo(name, out var propertyInfo))
            {
                SetDynamicCollection(name, collection);
                return;
            }

            if (propertyInfo.PropertyType.IsGenericType &&
                propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IList<>) &&
                typeof(ExtendableEntity).IsAssignableFrom(propertyInfo.PropertyType.GetGenericArguments().Single()))
            {
                propertyInfo.SetValue(this, collection);
                return;
            }

            throw new Exception($"Property [{name}] is not an entity collection");
        }

        public virtual IEntity GetEntity(string name)
        {
            if (!GetType().TryGetPropertyInfo(name, out var propertyInfo))
            {
                return GetDynamicEntity(name);
            }

            var value = propertyInfo.GetValue(this);
            if (null == value)
            {
                return null;
            }

            var entity = value as ExtendableEntity;
            if (null == entity)
            {
                throw new Exception($"Property [{name}] is not entity");
            }

            return entity;
        }

        public virtual void SetEntity(string name, IEntity entity)
        {
            SetDynamicEntity(name, entity);
        }

        public virtual T GetProperty<T>(string name)
        {
            if (GetType().TryGetPropertyInfo<T>(name, out var propertyInfo))
            {
                return (T) propertyInfo.GetValue(this);
            }

            return GetDynamicProperty<T>(name);
        }

        public virtual void SetProperty<T>(string name, T value)
        {
            if (GetType().TryGetPropertyInfo<T>(name, out var propertyInfo))
            {
                propertyInfo.SetValue(this, value);
                return;
            }

            SetDynamicProperty(name, value);
        }

        void IExtendableEntityConfiguration.SetDynamicProperty<T>(DynamicProperty<T> property)
        {
            Properties[property.Name] = property;
        }

        void IExtendableEntityConfiguration.SetDynamicEntity(string name, DynamicExtendableEntity entity)
        {
            throw new NotImplementedException();
        }

        protected T GetDynamicProperty<T>(string name)
        {
            if (Properties.TryGetValue(name, out var dynamicProperty))
            {
                return (T) dynamicProperty.Value;
            }

            throw new Exception($"Can not find property [{name}]");
        }

        protected void SetDynamicProperty<T>(string name, T value)
        {
            if (Properties.TryGetValue(name, out var dynamicProperty))
            {
                dynamicProperty.Value = value;
                return;
            }

            throw new Exception($"Can not find property [{name}]");
        }

        protected IList<IEntity> GetDynamicCollection(string name)
        {
            if (Collections.TryGetValue(name, out var list))
            {
                return list;
            }

            throw new Exception($"Can not find entity collection [{name}]");
        }

        protected void SetDynamicCollection(string name, IList<IEntity> collection)
        {
            Collections[name] = collection;
        }

        protected DynamicExtendableEntity GetDynamicEntity(string name)
        {
            if (Entities.TryGetValue(name, out var dynamicEntity))
            {
                return dynamicEntity;
            }

            throw new Exception($"Can not find entity property [{name}]");
        }

        protected virtual void SetDynamicEntity(string name, IEntity entity)
        {
            if (!(entity is DynamicExtendableEntity dynamicExtendableEntity))
            {
                throw new Exception($"Can not set entity [{name}] a value of non dynamic extendable entity");
            }

            Entities[name] = dynamicExtendableEntity;
        }
    }
}