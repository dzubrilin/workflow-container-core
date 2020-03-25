using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Diadem.Core.DomainModel
{
    public class JsonState
    {
        private readonly JObject _jObject;

        public const string EmptySerializedJsonState = "{}";

        public JsonState()
        {
            _jObject = new JObject();
        }

        public JsonState(string json)
        {
            _jObject = JObject.Parse(json);
        }
        
        internal JsonState(JObject jObject)
        {
            _jObject = jObject;
        }
        
        public bool IsEmpty => !_jObject.Properties().Any();

        public IList<T> GetCollection<T>(string name)
        {
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            var jProperty = _jObject.Property(name);
            if (null == jProperty)
            {
                throw new Exception($"Can not find property [{name}]");
            }

            if (jProperty.Value.Type != JTokenType.Array)
            {
                throw new Exception($"Can not access collection [{name}] since it is not array");
            }

            var jArray = (JArray) jProperty.Value;
            var result = new List<T>(jArray.Count);
            result.AddRange(jArray.Values<T>());
            return result;
        }

        public T GetProperty<T>(string name)
        {
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            var jProperty = _jObject.Property(name);
            if (null == jProperty)
            {
                throw new Exception($"Can not find property [{name}]");
            }

            if (jProperty.Value.Type == JTokenType.Array)
            {
                throw new Exception($"Can not access property [{name}] since it is array");
            }

            return jProperty.Value.Value<T>();
        }

        public void SetCollection<T>(string name, IList<T> values)
        {
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));
            Guard.ArgumentNotNull(values, nameof(values));

            var jArray = new JArray();
            foreach (var value in values)
            {
                jArray.Add(new JValue(value));
            }

            _jObject[name] = jArray;
        }

        public void SetProperty<T>(string name, T value)
        {
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));
            _jObject[name] = new JValue(value);
        }

        public string ToJsonString()
        {
            return _jObject.ToString(Formatting.None);
        }

        public void Merge(JsonState jsonState)
        {
            if (null == jsonState)
            {
                return;
            }

            var properties = jsonState._jObject.Properties();
            foreach (var property in properties)
            {
                _jObject[property.Name] = property.Value;
            }
        }

        public void MergeExcept(JsonState jsonState, IList<string> propertyNamesToSkip)
        {
            Guard.ArgumentNotNull(propertyNamesToSkip, nameof(propertyNamesToSkip));
            if (null == jsonState)
            {
                return;
            }

            var properties = jsonState._jObject.Properties();
            foreach (var property in properties)
            {
                if (propertyNamesToSkip.Contains(property.Name))
                {
                    continue;
                }

                _jObject[property.Name] = property.Value;
            }
        }
    }
}