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

        private int TotalDays()
        {
            return (EndDate - StartDate).Days + 1;
        }

        private DateTime EffectiveEndDate(Period period)
        {
            return EndDate < period.EndDate ? EndDate : period.EndDate;
        }

        private DateTime EffectiveStartDate(Period period)
        {
            return StartDate > period.StartDate ? StartDate : period.StartDate;
        }

        private Period OverlappingPeriod(Period period)
        {
            var effectiveStartDate = EffectiveStartDate(period);

            var effectiveEndDate = EffectiveEndDate(period);
            
            return new Period(effectiveStartDate, effectiveEndDate);
        }

        public int OverlappingDays(Period period)
        {
            if (EndDate < period.StartDate || StartDate > period.EndDate)
                return 0;

            return OverlappingPeriod(period).TotalDays();
        }
    }
}