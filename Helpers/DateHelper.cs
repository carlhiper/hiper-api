using System;

namespace Hiper.Api.Helpers
{
    public static class DateHelper
    {
        public static int GetMonthDifference(DateTime startDate, DateTime endDate)
        {
            var monthsApart = 12*(startDate.Year - endDate.Year) + startDate.Month - endDate.Month;
            return Math.Abs(monthsApart);
        }

        public static DateTime FirstDateOfQuarter(int currQuarter, int year)
        {
            var dtFirstDay = new DateTime(year, 3*currQuarter - 2, 1);

            return dtFirstDay;
        }

        public static DateTime LastDateOfQuarter(int currQuarter, int year)
        {
            var dtLastDay = new DateTime(year, 3*currQuarter, 1).AddDays(-1);
            return dtLastDay;
        }

        public static int GetQuarter(DateTime dateTime)
        {
            var currQuarter = (dateTime.Month - 1)/3 + 1;
            return currQuarter;
        }
    }
}