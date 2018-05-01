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

        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        public int TotalDays()
        {
            return (this.EndDate - this.StartDate).Days + 1;
        }

        public Period OverlappingPeriod(Period period)
        {
            var effectiveStart = StartDate > period.StartDate ? StartDate : period.StartDate;
            var effectiveEnd = EndDate < period.EndDate ? EndDate : period.EndDate;
            return new Period(effectiveStart, effectiveEnd);
        }

        public int OverlappingDays(Period period)
        {
            if (HasNoOverlapping(period))
            {
                return 0;
            }
            var effectiveStart = StartDate > period.StartDate ? StartDate : period.StartDate;
            var effectiveEnd = EndDate < period.EndDate ? EndDate : period.EndDate;

            return new Period(effectiveStart, effectiveEnd).TotalDays();
        }

        private bool HasNoOverlapping(Period period)
        {
            return EndDate < period.StartDate || StartDate > period.EndDate;
        }
    }
}