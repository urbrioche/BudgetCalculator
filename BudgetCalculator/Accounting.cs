﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

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

        public Period OverlappingPeriod(Period period)
        {
            var effectiveStart = StartDate > period.StartDate ? StartDate : period.StartDate;
            var effectiveEnd = EndDate < period.EndDate ? EndDate : period.EndDate;
            return new Period(effectiveStart, effectiveEnd);            
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
            var budgets = this._repo.GetAll();
            return GetRangeMonthAmount(new Period(start, end), budgets);
            //return period.IsSameMonth()
            //    ? GetOneMonthAmount(new Period(start, end), budgets)
            //    : GetRangeMonthAmount(new Period(start, end), budgets);
        }

        private decimal GetRangeMonthAmount(Period period, List<Budget> budgets)
        {
            var monthCount = period.MonthCount();
            var total = 0;

            foreach (var budget in budgets)
            {
                var effectivePeriod = period.OverlappingPeriod(new Period(budget.FirstDay, budget.LastDay));
                var amount = DailyAmount(effectivePeriod, budget) * effectivePeriod.EffectiveDays();
                total += amount;
            }

            return total;

            //for (var index = 0; index <= monthCount; index++)
            //{
            //    var budget = GetBudgetByCurrentPeriod(period, budgets, index);
            //    if (budget == null)
            //    {
            //        continue;
            //    }
            //    var effectivePeriod = period.OverlappingPeriod(budget);
            //    ////var effectiveDays = (effectivePeriod.EndDate.AddDays(1) - effectivePeriod.StartDate).Days;
            //    var amount = DailyAmount(effectivePeriod, budget) * effectivePeriod.EffectiveDays();
            //    total += amount;

            //    //total += GetOneMonthAmount(effectivePeriod, budgets);
            //}
            //return total;
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