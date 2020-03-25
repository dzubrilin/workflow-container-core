using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using Diadem.Core;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;

namespace Diadem.TimeZone
{
    public class NodaTimeZoneProvider : ITimeZoneProvider
    {
        private readonly ConcurrentDictionary<string, string> _ianaToWindowTimeZoneNameMap = new ConcurrentDictionary<string, string>();
        
        public string TranslateIanaToWindows(string ianaTimeZoneId)
        {
            Guard.ArgumentNotNullOrEmpty(ianaTimeZoneId, nameof(ianaTimeZoneId));
            return _ianaToWindowTimeZoneNameMap.GetOrAdd(ianaTimeZoneId, TranslateIanaToWindowsInternal);
        }

        public DateTime ConvertToLocal(DateTime dateTimeUtc, string ianaTimeZoneId)
        {
            Guard.ArgumentNotNullOrEmpty(ianaTimeZoneId, nameof(ianaTimeZoneId));
            Guard.ArgumentIsTrue(() => dateTimeUtc.Kind != DateTimeKind.Local, "DateTime.Kind must not be Local");
            
            var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(ianaTimeZoneId);
            if (null == timeZone)
            {
                throw new TimeZoneNotFoundException($"Unable to detect the timezone for {ianaTimeZoneId}");
            }
            
            var zonedDateTime = Instant.FromDateTimeUtc(dateTimeUtc).InZone(timeZone);
            return zonedDateTime.ToDateTimeUnspecified();
        }

        public DateTime ConvertToUtc(DateTime dateTimeLocal, string ianaTimeZoneId)
        {
            Guard.ArgumentNotNullOrEmpty(ianaTimeZoneId, nameof(ianaTimeZoneId));
            Guard.ArgumentIsTrue(() => dateTimeLocal.Kind != DateTimeKind.Utc, "DateTime.Kind must not be UTC");
            
            var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(ianaTimeZoneId);
            if (null == timeZone)
            {
                throw new TimeZoneNotFoundException($"Unable to detect the timezone for {ianaTimeZoneId}");
            }
            
            var localDateTime = dateTimeLocal.ToLocalDateTime();
            var zonedDateTime = localDateTime.InZoneLeniently(timeZone);
            var instant = zonedDateTime.ToInstant();
            var zonedInUtc = instant.InUtc();
            return zonedInUtc.ToDateTimeUtc();           
        }
        
        private static string TranslateIanaToWindowsInternal(string ianaTimeZoneId)
        {
            var timeZoneSource = TzdbDateTimeZoneSource.Default;
            var zoneMaps = timeZoneSource.WindowsMapping.MapZones;

            var mapZoneIds = timeZoneSource.CanonicalIdMap.Where(map => map.Value.Equals(ianaTimeZoneId, StringComparison.OrdinalIgnoreCase)).Select(x => x.Key).ToList();
            var mapZone = zoneMaps.FirstOrDefault(zoneMap => zoneMap.TzdbIds.Any(mapZoneIds.Contains));
            if (mapZone == null)
            {
                throw new TimeZoneNotFoundException($"Unable to detect the timezone for {ianaTimeZoneId}");
            }

            TimeZoneInfo timeZoneInfo = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(mapZone.WindowsId);                
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                foreach (var tzDbId in zoneMaps.SelectMany(zm => zm.TzdbIds))
                {
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(tzDbId);
                    if (null != timeZoneInfo)
                    {
                        break;
                    }
                }
            }
            
            if (null == timeZoneInfo)
            {
                throw new TimeZoneNotFoundException($"Unable to detect the timezone info for {mapZone.WindowsId}");
            }

            return timeZoneInfo.DisplayName;
        }
    }
}