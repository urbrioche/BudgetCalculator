using System;
using System.Collections.Generic;
using System.Linq;

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

        public bool IsSameMonth()
        {
            return StartDate.Year == EndDate.Year && StartDate.Month == EndDate.Month;
        }

        public int TotalDays()
        {
            return (EndDate - StartDate).Days + 1;
        }

        public DateTime EffectiveEndDate(Period period)
        {
            return EndDate < period.EndDate ? EndDate : period.EndDate;
        }

        public DateTime EffectiveStartDate(Period period)
        {
            return StartDate > period.StartDate ? StartDate : period.StartDate;
        }

        public Period OverlappingPeriod(Period period)
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

    internal class Accounting
    {
        private readonly IRepository<Budget> _repo;

        public Accounting(IRepository<Budget> repo)
        {
            _repo = repo;
        }

        public decimal TotalAmount(DateTime startDate, DateTime endDate)
        {

            var period = new Period(startDate, endDate);
            var budgets = this._repo.GetAll();
            return GetRangeMonthAmount(period, budgets);
            return period.IsSameMonth()
                ? GetOneMonthAmount(period, budgets)
                : GetRangeMonthAmount(period, budgets);
        }

        private decimal GetRangeMonthAmount(Period period, List<Budget> budgets)
        {
            var total = 0;

            foreach (var budget in budgets)
            {
                total += budget.EffectiveAmount(period);
            }

            return total;            
        }

        private Budget GetBudgetByCurrentPeriodMonth(Period period, List<Budget> budgets, int index)
        {
            return budgets.FirstOrDefault(r => r.YearMonth == period.StartDate.AddMonths(index).ToString("yyyyMM"));
        }

        private int GetOneMonthAmount(Period period, List<Budget> budgets)
        {
            var effectiveDays = period.TotalDays();

            var budget = budgets.Get(period.StartDate);
            if (budget == null)
            {
                return 0;
            }

            var dailyAmount = budget.DailyAmount();
        
            return dailyAmount * effectiveDays;

        }
    }

    public static class BudgetExtension
    {
        public static Budget Get(this List<Budget> list, DateTime date)
        {
            return list.FirstOrDefault(r => r.YearMonth == date.ToString("yyyyMM"));
        }
    }

    public static class DateTimeExtension
    {
        public static int MonthDifference(this DateTime lValue, DateTime rValue)
        {
            return (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);
        }

        public static DateTime LastDate(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }

        public static DateTime FirstDate(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

    }
}