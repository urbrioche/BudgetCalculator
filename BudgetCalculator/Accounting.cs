using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace BudgetCalculator
{
    public class Period
    {
        public Period(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        public bool IsSameMonth()
        {
            return this.StartDate.Year == this.EndDate.Year && this.StartDate.Month == this.EndDate.Month;
        }

        public int TotalDays()
        {
            return (this.EndDate - this.StartDate).Days + 1;
        }

        public int MonthCount()
        {
            return EndDate.MonthDifference(StartDate);
        }

        public Period OverlappingPeriod(Period period)
        {
            var effectiveStart = StartDate > period.StartDate ? StartDate : period.StartDate;
            var effectiveEnd = EndDate < period.EndDate ? EndDate : period.EndDate;
            return new Period(effectiveStart, effectiveEnd);
        }

        public int OverlappingDays(Period period)
        {
            var effectiveStart = StartDate > period.StartDate ? StartDate : period.StartDate;
            var effectiveEnd = EndDate < period.EndDate ? EndDate : period.EndDate;
            return new Period(effectiveStart, effectiveEnd).TotalDays();
        }
    }

    public class Accounting
    {
        private readonly IRepository<Budget> _repo;

        public Accounting(IRepository<Budget> repo)
        {
            _repo = repo;
        }

        public decimal TotalAmount(DateTime start, DateTime end)
        {
            if (start > end)
            {
                throw new ArgumentException();
            }

            var budgets = this._repo.GetAll();
            return budgets.Sum(b => b.EffectiveAmount(new Period(start, end)));
            //return period.IsSameMonth()
            //    ? GetOneMonthAmount(new Period(start, end), budgets)
            //    : GetRangeMonthAmount(new Period(start, end), budgets);
        }

        private static Budget GetBudgetByCurrentPeriod(Period period, List<Budget> budgets, int index)
        {
            return budgets.FirstOrDefault(b => b.YearMonth == period.StartDate.AddMonths(index).ToString("yyyyMM"));
        }

        private int GetOneMonthAmount(Period period, List<Budget> budgets)
        {
            var budget = budgets.Get(period.StartDate);
            if (budget == null)
            {
                return 0;
            }
            return budget.DailyAmount() * period.TotalDays();
        }

        private static int DaysInMonth(Period period)
        {
            return DateTime.DaysInMonth(period.StartDate.Year, period.StartDate.Month);
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