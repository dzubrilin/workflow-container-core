using Newtonsoft.Json.Linq;

namespace Diadem.Core.DomainModel
{
    public static class JsonStateExtensions
    {
        public static JsonState AsJsonState(this string json)
        {
            return !string.IsNullOrEmpty(json) ? new JsonState(json) : new JsonState();
        }

        public static bool TryConvertToJsonState(this string json, out JsonState jsonState)
        {
            json = json.Trim();
            if (json.StartsWith("{") && json.EndsWith("}"))
            {
                try
                {
                    var jObject = JObject.Parse(json);
                    jsonState = new JsonState(jObject);
                    return true;
                }
                catch
                {
                    jsonState = null;
                    return false;
                }
            }

            jsonState = null;
            return false;
        }
    }
}