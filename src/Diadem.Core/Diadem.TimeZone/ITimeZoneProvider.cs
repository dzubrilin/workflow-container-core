using System;

namespace Diadem.TimeZone
{
    public interface ITimeZoneProvider
    {
        string TranslateIanaToWindows(string ianaTimeZoneId);

        DateTime ConvertToLocal(DateTime dateTimeUtc, string ianaTimeZoneId);
        
        DateTime ConvertToUtc(DateTime dateTimeLocal, string ianaTimeZoneId);
    }
}