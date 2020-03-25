namespace Diadem.Core.DomainModel
{
    public static class JsonEntityExtensions
    {
        public static JsonEntity AsJsonEntity(this string json)
        {
            return !string.IsNullOrEmpty(json) ? new JsonEntity(json) : new JsonEntity(string.Empty, string.Empty);
        }
    }
}