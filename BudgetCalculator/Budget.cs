using System;

namespace BudgetCalculator
{
    public class Budget
    {
        public string YearMonth { get; set; }

        public int Amount { get; set; }

        private int TotalDays
        {
            get
            {
                var firstDay = FirstDay;
                return DateTime.DaysInMonth(firstDay.Year, firstDay.Month);
            }
        }

        private DateTime FirstDay
        {
            get
            {
                return DateTime.ParseExact(YearMonth + "01", "yyyyMMdd", null);
            }
        }

        private DateTime LastDay
        {
            get
            {
                return DateTime.ParseExact(YearMonth + TotalDays, "yyyyMMdd", null);
            }
        }

        private int DailyAmount()
        {
            return Amount / TotalDays;
        }

        public int EffectiveAmount(Period period)
        {
            return DailyAmount() *
                   period.OverlappingDays(new Period(FirstDay, LastDay));
        }
    }
}