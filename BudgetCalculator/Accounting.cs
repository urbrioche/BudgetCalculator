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
            return period.IsSameMonth()
                ? GetOneMonthAmount(startDate, endDate)
                : GetRangeMonthAmount(startDate, endDate);
        }

        private decimal GetRangeMonthAmount(DateTime start, DateTime end)
        {
            var monthCount = end.MonthDifference(start);
            var total = 0;
            for (var index = 0; index <= monthCount; index++)
            {
                if (index == 0)
                {
                    total += GetOneMonthAmount(start, start.LastDate());
                }
                else if (index == monthCount)
                {
                    total += GetOneMonthAmount(end.FirstDate(), end);
                }
                else
                {
                    var now = start.AddMonths(index);
                    total += GetOneMonthAmount(now.FirstDate(), now.LastDate());
                }
            }
            return total;
        }

        private int GetOneMonthAmount(DateTime start, DateTime end)
        {
            var list = this._repo.GetAll();
            var budget = list.Get(start)?.Amount ?? 0;

            var days = DateTime.DaysInMonth(start.Year, start.Month);
            var validDays = GetValidDays(start, end);

            return (budget / days) * validDays;
        }

        private int GetValidDays(DateTime start, DateTime end)
        {
            return (end - start).Days + 1;
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