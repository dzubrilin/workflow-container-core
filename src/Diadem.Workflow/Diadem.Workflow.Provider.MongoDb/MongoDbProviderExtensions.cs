using System;

namespace Diadem.Workflow.Provider.MongoDb
{
    internal static class MongoDbProviderExtensions
    {
        internal static string ToIdString(this Guid guid)
        {
            return guid.ToString("N");
        }

        internal static Guid ToIdGuid(this string guid)
        {
            return new Guid(guid);
        }
    }
}