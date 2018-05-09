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
                var firstDay = DateTime.ParseExact(YearMonth + "01", "yyyyMMdd", null);
                return DateTime.DaysInMonth(firstDay.Year, firstDay.Month);

            }
        }

        public int DailyAmount()
        {
            return Amount / TotalDays;
        }
    }
}