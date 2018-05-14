using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetCalculator
{
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
            return period.IsSameMonth()
                ? GetOneMonthAmount(period, budgets)
                : GetRangeMonthAmount(startDate, endDate, period, budgets);
        }

        private decimal GetRangeMonthAmount(DateTime start, DateTime end, Period period, List<Budget> budgets)
        {
            var monthCount = period.MonthCount();
            var total = 0;
            for (var index = 0; index <= monthCount; index++)
            {
                var effectivePeriod = EffectivePeriod(start, end, index, monthCount);

                total += GetOneMonthAmount(effectivePeriod, this._repo.GetAll());
            }
            return total;
        }

        private static Period EffectivePeriod(DateTime start, DateTime end, int index, int monthCount)
        {
            Period effectivePeriod;
            if (index == 0)
            {
                effectivePeriod = new Period(start, start.LastDate());
            }
            else if (index == monthCount)
            {
                effectivePeriod = new Period(end.FirstDate(), end);
            }
            else
            {
                var now = start.AddMonths(index);
                effectivePeriod = new Period(now.FirstDate(), now.LastDate());
            }

            return effectivePeriod;
        }

        private int GetOneMonthAmount(Period period, List<Budget> budgets)
        {
            var budget = budgets.Get(period.StartDate);
            if (budget == null)
                return 0;

            return budget.DailyAmount() * period.TotalDays();
        }
    }

    internal class Period
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

        public bool IsSameMonth()
        {
            return this.StartDate.Year == this.EndDate.Year && this.StartDate.Month == this.EndDate.Month;
        }

        public int DaysInMonth()
        {
            return DateTime.DaysInMonth(StartDate.Year, StartDate.Month);
        }

        public int TotalDays()
        {
            return (this.EndDate - this.StartDate).Days + 1;
        }

        public int MonthCount()
        {
            return EndDate.MonthDifference(StartDate);
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