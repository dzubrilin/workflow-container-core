using System;
using System.Globalization;

namespace Diadem.Core.DomainModel
{
    public static class EntityId
    {
        public static string NewId()
        {
            return Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        }
    }
}