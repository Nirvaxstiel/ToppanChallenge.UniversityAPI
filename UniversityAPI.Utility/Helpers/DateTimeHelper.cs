using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace System
{
    public sealed class DateTimeHelper
    {
        public static IList<Week> GetWeeks(DateTime from, DateTime to, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
        {
            var dfi = DateTimeFormatInfo.CurrentInfo;
            var cal = dfi.Calendar;

            var start = from;
            var weeks = new List<Week>();
            while (start.Date < to.Date)
            {
                var week = new Week() { StartDate = start };

                var endOfWeek = GetSundayOfWeek(start);
                var end = to.Date > endOfWeek ? endOfWeek : to;

                week.Year = GetWeekYear(start, rule, firstDayOfWeek);
                week.EndDate = end;
                week.WeekOfYear = cal.GetWeekOfYear(start, rule, firstDayOfWeek);
                weeks.Add(week);

                start = end.AddDays(1);
            }

            return weeks;
        }

        public static IList<Week> GetWeeks(int year, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
        {
            var from = new DateTime(year, 1, 1);
            var to = new DateTime(year, 12, 31);
            var weeks = GetWeeks(from, to, rule, firstDayOfWeek);

            return weeks.Where(w => w.Year == year).ToList();
        }

        public static IList<DateTime> GetDates(DateTime startDate, DateTime endDate, IEnumerable<int> dayOfWeeks)
        {
            return InnerGetDates(startDate, endDate, dayOfWeeks).ToList();
        }

        public static DateTime GetMondayOfWeek(DateTime value)
        {
            int days = 1 - (int)value.DayOfWeek;

            return value.AddDays(days).Date;
        }

        public static string GetUtcTimestamp()
        {
            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long totalSeconds = (long)timeSpan.TotalSeconds;

            return totalSeconds.ToString();
        }

        public static DateTime GetSundayOfWeek(DateTime value)
        {
            if (value.DayOfWeek == DayOfWeek.Sunday)
            {
                return value;
            }

            int days = 7 - (int)value.DayOfWeek;
            return value.AddDays(days).Date.AddDays(1).AddSeconds(-1);
        }

        public static DateTime GetFirstDateOfMonth(DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1).Date;
        }

        public static DateTime GetLastDateOfMonth(DateTime value)
        {
            DateTime firstDayOfTheMonth = new DateTime(value.Year, value.Month, 1).Date;

            return firstDayOfTheMonth.AddMonths(1).AddDays(-1).Date;
        }

        public static DateTime TimeZoneNow(string timeZoneId)
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        }

        public static DateTime StandardNow()
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, TimeZoneHelper.StandardTimeZone);
        }

        public static DateTime StandardDateTime(DateTime dateTime, string timeZoneId)
        {
            return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId), TimeZoneHelper.StandardTimeZone);
        }

        public static string GetTimestamp()
        {
            return StandardNow().ToString("yyyyMMddHHmmssfff");
        }

        public static string GetStringDate()
        {
            return StandardNow().ToString("ddMMyyyy");
        }

        private static int GetWeekYear(DateTime value, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
        {
            if (rule == CalendarWeekRule.FirstDay || rule == CalendarWeekRule.FirstFullWeek)
            {
                return value.AddDays((int)firstDayOfWeek - (int)value.DayOfWeek).Year;
            }

            if ((int)value.DayOfWeek <= (int)DayOfWeek.Thursday && (int)value.DayOfWeek > 0)
            {
                return value.Year;
            }

            return value.AddDays(-3).Year;
        }

        private static IEnumerable<DateTime> InnerGetDates(DateTime startDate, DateTime endDate, IEnumerable<int> dayOfWeeks)
        {
            var date = startDate;

            while (date <= endDate)
            {
                if (dayOfWeeks.Contains((int)date.DayOfWeek))
                {
                    yield return date;
                }

                date = date.AddDays(1);
            }
        }

        public static double GetRemainingMinutes(DateTime endDate, DateTime startDate)
        {
            var span = endDate - startDate;
            var minute = span.TotalMinutes;

            minute = span.TotalSeconds > 50 ? minute + 1 : minute;
            return minute == 0 ? 1 : Math.Truncate(minute);
        }
    }

    public class Week
    {
        public int Year
        { get; set; }

        public int WeekOfYear
        { get; set; }

        public DateTime StartDate
        { get; set; }

        public DateTime EndDate
        { get; set; }
    }
}