﻿using System;
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
                : GetRangeMonthAmount(period, budgets);
        }

        private decimal GetRangeMonthAmount(Period period, List<Budget> budgets)
        {
            var monthCount = period.MonthCount();
            var total = 0;
            for (var index = 0; index <= monthCount; index++)
            {
                Budget budget = GetCurrentBudgetByPeriodMonth(period, index, budgets);
                if (budget == null)
                    continue;
                var effectivePeriod = EffectivePeriod(index, monthCount, period, budget);

                total += GetOneMonthAmount(effectivePeriod, budgets);
            }
            return total;
        }

        private Budget GetCurrentBudgetByPeriodMonth(Period period, int index, List<Budget> budgets)
        {
            var periodMonth = period.StartDate.AddMonths(index).ToString("yyyyMM");
            return budgets.FirstOrDefault(b => b.YearMonth == periodMonth);
        }

        private static Period EffectivePeriod(int index, int monthCount, Period period, Budget budget)
        {
            Period effectivePeriod;
            DateTime effectiveStartDate = budget.FirstDay;
            DateTime effectiveEndDate = budget.LastDay;
            if (period.StartDate > budget.FirstDay)
            {
                effectiveStartDate = period.StartDate;
            }

            if (period.EndDate < budget.LastDay)
            {
                effectiveEndDate = period.EndDate;
            }

            return new Period(effectiveStartDate, effectiveEndDate);            
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