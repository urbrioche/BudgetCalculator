using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetCalculator
{
    internal class Period
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

        public int EffectiveDays()
        {
            return (this.EndDate - this.StartDate).Days + 1;
        }

        public int MonthCount()
        {
            return EndDate.MonthDifference(StartDate);
        }
    }

    internal class Accounting
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

            var period = new Period(start, end);
            return period.IsSameMonth()
                ? GetOneMonthAmount(new Period(start, end))
                : GetRangeMonthAmount(new Period(start, end));
        }

        private decimal GetRangeMonthAmount(Period period)
        {
            var monthCount = period.MonthCount();
            var total = 0;
            for (var index = 0; index <= monthCount; index++)
            {
                var effectivePeriod = EffectivePeriod(period, index, monthCount);
                total += GetOneMonthAmount(effectivePeriod);
            }
            return total;
        }

        private static Period EffectivePeriod(Period period, int index, int monthCount)
        {
            Period effectivePeriod;
            if (index == 0)
            {
                effectivePeriod = new Period(period.StartDate, period.StartDate.LastDate());
            }
            else if (index == monthCount)
            {
                effectivePeriod = new Period(period.EndDate.FirstDate(), period.EndDate);
            }
            else
            {
                var now = period.StartDate.AddMonths(index);
                effectivePeriod = new Period(now.FirstDate(), now.LastDate());
            }

            return effectivePeriod;
        }

        private int GetOneMonthAmount(Period period)
        {
            var budgets = this._repo.GetAll();
            var budget = budgets.Get(period.StartDate);
            if (budget == null)
            {
                return 0;
            }
            return DailyAmount(period, budget) * period.EffectiveDays();
        }

        private static int DailyAmount(Period period, Budget budget)
        {
            return budget.Amount / DaysInMonth(period);
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