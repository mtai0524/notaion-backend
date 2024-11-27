namespace Notaion.Application.Common.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public static DateTime GetVietnamTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
        }
        public static string GetCurrentYear()
        {
            return DateTime.Now.Year.ToString();
        }

        public static DateTime ConvertToVietnamTime(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTime(dateTime, VietnamTimeZone);
        }

        public static DateTime ConvertToUtcFromVietnamTime(DateTime vietnamTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(vietnamTime, VietnamTimeZone);
        }
    }
}
