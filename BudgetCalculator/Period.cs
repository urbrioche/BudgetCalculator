using System;

namespace BudgetCalculator
{
    public class Period
    {
        public Period(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException();
            }
            StartDate = startDate;
            EndDate = endDate;
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        private int DaysInMonth()
        {
            return DateTime.DaysInMonth(StartDate.Year, StartDate.Month);
        }

        private int TotalDays()
        {
            return (this.EndDate - this.StartDate).Days + 1;
        }

        public int OverlappingDays(Period period)
        {
            if (EndDate < period.StartDate || StartDate > period.EndDate)
            {
                return 0;
            }

            return OverlappingPeriod(period).TotalDays();
        }

        private Period OverlappingPeriod(Period period)
        {
            DateTime effectiveStartDate = period.StartDate;
            DateTime effectiveEndDate = period.EndDate;
            if (StartDate > period.StartDate)
            {
                effectiveStartDate = StartDate;
            }

            if (EndDate < period.EndDate)
            {
                effectiveEndDate = EndDate;
            }

            return new Period(effectiveStartDate, effectiveEndDate);
        }
    }
}