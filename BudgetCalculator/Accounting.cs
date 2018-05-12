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
            var budgets = this._repo.GetAll();
            return period.IsSameMonth()
                ? GetOneMonthAmount(period, budgets)
                : GetRangeMonthAmount(period, budgets);
        }

        private decimal GetRangeMonthAmount(Period period, List<Budget> budgets)
        {
            var monthCount = period.EndDate.MonthDifference(period.StartDate);
            var total = 0;
            for (var index = 0; index <= monthCount; index++)
            {
                var currentBudget = GetBudgetByCurrentPeriodMonth(period, budgets, index);
                if (currentBudget == null)
                    continue;
                var effectivePeriod = EffectivePeriod(index, monthCount, period, currentBudget);

                total += GetOneMonthAmount(effectivePeriod, budgets);
            }
            return total;
        }

        private Budget GetBudgetByCurrentPeriodMonth(Period period, List<Budget> budgets, int index)
        {
            return budgets.FirstOrDefault(r => r.YearMonth == period.StartDate.AddMonths(index).ToString("yyyyMM"));
        }

        private static Period EffectivePeriod(int index, int monthCount, Period period, Budget budget)
        {
            var effectiveStartDate = EffectiveStartDate(period, budget);

            var effectiveEndDate = EffectiveEndDate(period, budget);
            
            return new Period(effectiveStartDate, effectiveEndDate);
        }

        private static DateTime EffectiveEndDate(Period period, Budget budget)
        {
            return period.EndDate < budget.LastDay ? period.EndDate : budget.LastDay;
        }

        private static DateTime EffectiveStartDate(Period period, Budget budget)
        {
            return period.StartDate > budget.FirstDay ? period.StartDate : budget.FirstDay;
        }

        private int GetOneMonthAmount(Period period, List<Budget> budgets)
        {
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