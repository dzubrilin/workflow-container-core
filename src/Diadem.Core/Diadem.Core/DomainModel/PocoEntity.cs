using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Diadem.Core.Common;

namespace Diadem.Core.DomainModel
{
    /// <summary>
    ///     Is supposed to be used as base class for POCO domain entity implementation
    /// </summary>
    public abstract class PocoEntity : ExtendableEntity, IExtendableEntity
    {
        protected PocoEntity()
        {
        }

        protected PocoEntity(string entityId)
        {
            EntityId = entityId;
        }

        public override string EntityId { get; set; }

        public override IList<IEntity> GetCollection(string name)
        {
            if (!GetType().TryGetPropertyInfo(name, out var propertyInfo))
            {
                throw new Exception($"Can not find property [{name}]");
            }

            var value = propertyInfo.GetValue(this);
            if (null == value)
            {
                return null;
            }

            if (propertyInfo.PropertyType.IsGenericType)
            {
                var genericTypeDefinition = propertyInfo.PropertyType.GetGenericTypeDefinition();
                if ((genericTypeDefinition == typeof(IList<>) || genericTypeDefinition == typeof(List<>)) &&
                    typeof(PocoEntity).IsAssignableFrom(propertyInfo.PropertyType.GetGenericArguments().Single()))
                {
                    return ((IList)value).Cast<IEntity>().ToList();
                }
            }

            throw new Exception($"Property [{name}] is not an entity collection or its type is not inherited from [PocoEntity]");
        }

        public override IEntity GetEntity(string name)
        {
            if (!GetType().TryGetPropertyInfo(name, out var propertyInfo))
            {
                throw new Exception($"Can not find property [{name}]");
            }

            var value = propertyInfo.GetValue(this);
            if (null == value)
            {
                return null;
            }

            var entity = value as PocoEntity;
            if (null == entity)
            {
                throw new Exception($"Property [{name}] is not entity");
            }

            return entity;
        }

        public override T GetProperty<T>(string name)
        {
            if (!GetType().TryGetPropertyInfo<T>(name, out var propertyInfo))
            {
                throw new Exception($"Can not find property [{name}]");
            }

            return (T) propertyInfo.GetValue(this);
        }

        public override void SetProperty<T>(string name, T value)
        {
            if (!GetType().TryGetPropertyInfo<T>(name, out var propertyInfo))
            {
                throw new Exception($"Can not find property [{name}]");
            }

            propertyInfo.SetValue(this, value);
        }
    }
}