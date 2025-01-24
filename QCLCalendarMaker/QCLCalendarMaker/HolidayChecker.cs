using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QCLCalendarMaker
{
    public static class USHolidays
    {
        public static List<DateTime> GetFederalHolidays(int year)
        {
            var holidays = new List<DateTime>
        {
            NewYearsDay(year),
            MartinLutherKingJrDay(year),
            PresidentsDay(year),
            MemorialDay(year),
            Juneteenth(year),
            IndependenceDay(year),
            LaborDay(year),
            ColumbusDay(year),
            VeteransDay(year),
            ThanksgivingDay(year),
            ChristmasDay(year)
        };

            return holidays.OrderBy(h => h).ToList();
        }

        private static DateTime NewYearsDay(int year) => new DateTime(year, 1, 1);
        private static DateTime Juneteenth(int year) => new DateTime(year, 6, 19);
        private static DateTime IndependenceDay(int year) => new DateTime(year, 7, 4);
        private static DateTime VeteransDay(int year) => new DateTime(year, 11, 11);
        private static DateTime ChristmasDay(int year) => new DateTime(year, 12, 25);

        // MLK Day: 3rd Monday in January
        private static DateTime MartinLutherKingJrDay(int year)
            => NthWeekdayOfMonth(year, 1, DayOfWeek.Monday, 3);

        // Presidents Day: 3rd Monday in February
        private static DateTime PresidentsDay(int year)
            => NthWeekdayOfMonth(year, 2, DayOfWeek.Monday, 3);

        // Memorial Day: last Monday of May
        private static DateTime MemorialDay(int year)
            => LastWeekdayOfMonth(year, 5, DayOfWeek.Monday);

        // Labor Day: 1st Monday in September
        private static DateTime LaborDay(int year)
            => NthWeekdayOfMonth(year, 9, DayOfWeek.Monday, 1);

        // Columbus Day: 2nd Monday in October
        private static DateTime ColumbusDay(int year)
            => NthWeekdayOfMonth(year, 10, DayOfWeek.Monday, 2);

        // Thanksgiving: 4th Thursday in November
        private static DateTime ThanksgivingDay(int year)
            => NthWeekdayOfMonth(year, 11, DayOfWeek.Thursday, 4);

        /// <summary>
        /// Returns the date of the nth occurrence of a given DayOfWeek in a specified month/year.
        /// For example, 3rd Monday of January.
        /// </summary>
        private static DateTime NthWeekdayOfMonth(int year, int month, DayOfWeek dayOfWeek, int nth)
        {
            // Start on the 1st day of that month.
            DateTime firstOfMonth = new DateTime(year, month, 1);

            // How many days until we get the desired dayOfWeek?
            int offset = (int)dayOfWeek - (int)firstOfMonth.DayOfWeek;
            if (offset < 0) offset += 7;

            // 'offset' is how many days from the 1st to the first occurrence of dayOfWeek
            DateTime firstOccurrence = firstOfMonth.AddDays(offset);

            // Now add 7 days * (nth - 1) to get the nth occurrence
            return firstOccurrence.AddDays(7 * (nth - 1));
        }

        /// <summary>
        /// Returns the date of the last occurrence of a given DayOfWeek in a specified month/year.
        /// Example: last Monday of May
        /// </summary>
        private static DateTime LastWeekdayOfMonth(int year, int month, DayOfWeek dayOfWeek)
        {
            // Start on the last day of that month
            DateTime lastOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            // Move backwards until we find the desired dayOfWeek
            while (lastOfMonth.DayOfWeek != dayOfWeek)
            {
                lastOfMonth = lastOfMonth.AddDays(-1);
            }
            return lastOfMonth;
        }
    }
}
