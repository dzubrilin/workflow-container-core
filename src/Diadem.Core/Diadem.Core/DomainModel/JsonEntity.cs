using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Diadem.Core.DomainModel
{
    public class JsonEntity : IExtendableEntity
    {
        private readonly JObject _jObject;

        public JsonEntity()
        {
            _jObject = new JObject();
        }

        public JsonEntity(string json)
        {
            _jObject = JObject.Parse(json);
        }

        public JsonEntity(string entityType, string entityId)
        {
            _jObject = new JObject();
            EntityType = entityType;
            EntityId = entityId;
        }

        public JsonEntity(string entityType, string entityId, string json)
        {
            _jObject = JObject.Parse(json);
            EntityType = entityType;
            EntityId = entityId;
        }

        private JsonEntity(JObject jObject)
        {
            _jObject = jObject;
        }

        public string EntityId
        {
            get => GetProperty<string>("EntityId");
            set => SetProperty("EntityId", value);
        }

        public string EntityType
        {
            get => GetProperty<string>("EntityType");
            private set => SetProperty("EntityType", value);
        }

        public IList<IEntity> GetCollection(string name)
        {
            var jProperty = _jObject.Property(name);
            if (null == jProperty)
            {
                throw new Exception($"Can not find property [{name}]");
            }

            var value = jProperty.Value;
            if (null == value)
            {
                throw new NullReferenceException($"Can not find property [{name}]");
            }

            var jArray = value as JArray;
            if (null == jArray)
            {
                throw new Exception($"Can not find array property [{name}]");
            }

            IList<IEntity> list = new List<IEntity>(jArray.Count);
            foreach (var jToken in jArray)
            {
                var jObject = jToken as JObject;
                if (null == jObject)
                {
                    throw new Exception($"Property [{name}] contains non entity elements");
                }

                list.Add(new JsonEntity(jObject));
            }

            return list;
        }

        public void SetCollection(string name, IList<IEntity> collection)
        {
            var jProperty = _jObject.Property(name);
            if (null != jProperty)
            {
                if (jProperty.Value is JArray)
                {
                    _jObject.Remove(name);
                }
                else
                {
                    throw new Exception($"The property [{name}] is not a collection, can not set collection into it");
                }
            }

            var jArray = new JArray();
            foreach (var entity in collection)
            {
                if (entity is JsonEntity jsonEntity)
                {
                    jArray.Add(jsonEntity._jObject);
                }
                else
                {
                    throw new Exception($"Can not set the property [{name}] as a collection since it contains a non JSonEntity");
                }
            }

            _jObject[name] = jArray;
        }

        public IEntity GetEntity(string name)
        {
            var jProperty = _jObject.Property(name);
            if (null == jProperty)
            {
                throw new Exception($"Can not find property [{name}]");
            }

            var value = jProperty.Value;
            if (null == value)
            {
                throw new NullReferenceException($"Can not find property [{name}]");
            }

            var jObject = value as JObject;
            if (null == jObject)
            {
                throw new Exception($"Can not find entity property [{name}]");
            }

            return new JsonEntity(jObject);
        }

        public T GetProperty<T>(string name)
        {
            var jProperty = _jObject.Property(name);
            if (null == jProperty)
            {
                throw new Exception($"Can not find property [{name}]");
            }

            return jProperty.Value.Value<T>();
        }

        public void SetEntity(string name, IEntity entity)
        {
            if (!(entity is JsonEntity jsonEntity))
            {
                throw new Exception("Only JsonEntity are supported");
            }

            _jObject[name] = jsonEntity._jObject;
        }

        public void SetProperty<T>(string name, T value)
        {
            _jObject[name] = new JValue(value);
        }

        public string ToJsonString()
        {
            return _jObject.ToString(Formatting.None);
        }
    }
}