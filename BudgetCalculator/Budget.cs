using System;

namespace BudgetCalculator
{
    public class Budget
    {
        public string YearMonth { get; set; }

        public int Amount { get; set; }

        public int TotalDays
        {
            get
            {
                var firstDay = FirstDay;
                return DateTime.DaysInMonth(firstDay.Year, firstDay.Month);
            }
        }

        public DateTime FirstDay
        {
            get
            {
                return DateTime.ParseExact(YearMonth + "01", "yyyyMMdd", null);
            }
        }

        public DateTime LastDay
        {
            get
            {
                return DateTime.ParseExact(YearMonth + TotalDays, "yyyyMMdd", null);
            }
        }

        //public DateTime LastDay
        //{
        //    get
        //    {

        //    }
        //}

        public int DailyAmount()
        {
            return Amount / TotalDays;
        }
    }
}