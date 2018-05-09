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
            return StartDate.Year == EndDate.Year && StartDate.Month == EndDate.Month;
        }

        public int EffectiveDays()
        {
            return (EndDate - StartDate).Days + 1;
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
            if (startDate > endDate)
            {
                throw new ArgumentException();
            }

            var period = new Period(startDate, endDate);
            return period.IsSameMonth()
                ? GetOneMonthAmount(period)
                : GetRangeMonthAmount(startDate, endDate, period);
        }

        private decimal GetRangeMonthAmount(DateTime start, DateTime end, Period period)
        {
            var monthCount = end.MonthDifference(start);
            var total = 0;
            for (var index = 0; index <= monthCount; index++)
            {
                if (index == 0)
                {
                    period = new Period(start, start.LastDate());
                    total += GetOneMonthAmount(period);
                }
                else if (index == monthCount)
                {
                    period = new Period(end.FirstDate(), end);
                    total += GetOneMonthAmount(period);
                }
                else
                {
                    var now = start.AddMonths(index);
                    period = new Period(now.FirstDate(), now.LastDate());
                    total += GetOneMonthAmount(period);
                }
            }
            return total;
        }

        private int GetOneMonthAmount(Period period)
        {
            var budgets = this._repo.GetAll();
            var effectiveDays = period.EffectiveDays();

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