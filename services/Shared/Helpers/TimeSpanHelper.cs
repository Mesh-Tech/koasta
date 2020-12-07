using System;
using TimeZoneConverter;

namespace Koasta.Shared.Helpers
{
    public static class TimeSpanHelper
    {
        public static TimeSpan LondonNow() {
            var dt = DateTime.UtcNow;
            var zone = TZConvert.GetTimeZoneInfo("Europe/London");
            var offset = zone.GetUtcOffset(dt);

            return new TimeSpan(dt.Hour, dt.Minute, dt.Second).Add(offset);
        }
    }
}
