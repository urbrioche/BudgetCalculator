using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BudgetCalculator
{
    [TestClass]
    public class UnitTest1
    {
        private readonly IRepository<Budget> _repository = Substitute.For<IRepository<Budget>>();
        private Accounting _accounting;

        [TestInitialize]
        public void TestInit()
        {
            _accounting = new Accounting(_repository);
        }

        [TestMethod]
        public void no_budgets()
        {
            GivenBudgets();
            TotalAmountShouldBe(0, new DateTime(2018, 3, 1), new DateTime(2018, 3, 1));
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void invalid_period()
        {
            GivenBudgets();
            TotalAmountShouldBe(0, new DateTime(2018, 3, 1), new DateTime(2018, 2, 1));
        }

        [TestMethod]
        public void period_equals_to_budget_month()
        {
            GivenBudgets(new Budget() { YearMonth = "201801", Amount = 62 });
            TotalAmountShouldBe(62, new DateTime(2018, 1, 1), new DateTime(2018, 1, 31));
        }

        [TestMethod]
        public void fifteen_effective_days_period_inside_budget_month()
        {
            GivenBudgets(new Budget() { YearMonth = "201801", Amount = 62 });
            TotalAmountShouldBe(30, new DateTime(2018, 1, 1), new DateTime(2018, 1, 15));
        }

        [TestMethod]
        public void no_effective_days_period_after_budget_month_last_day()
        {
            GivenBudgets(new Budget() { YearMonth = "201801", Amount = 62 });
            TotalAmountShouldBe(0, new DateTime(2018, 2, 1), new DateTime(2018, 2, 15));
        }

        [TestMethod]
        public void no_effective_days_period_before_budget_month_first_day()
        {
            GivenBudgets(new Budget() { YearMonth = "201801", Amount = 62 });
            TotalAmountShouldBe(0, new DateTime(2017, 12, 1), new DateTime(2017, 12, 15));
        }

        [TestMethod]
        public void period_equals_to_two_budget_month()
        {
            GivenBudgets(new Budget() { YearMonth = "201801", Amount = 62 },
                new Budget() { YearMonth = "201802", Amount = 280 });
            TotalAmountShouldBe(342, new DateTime(2018, 1, 1), new DateTime(2018, 2, 28));
        }

        [TestMethod]
        public void period_cross_three_budget_month()
        {
            GivenBudgets(new Budget() { YearMonth = "201801", Amount = 62 },
                new Budget() { YearMonth = "201802", Amount = 280 },
                new Budget() { YearMonth = "201803", Amount = 62 });
            TotalAmountShouldBe(362, new DateTime(2018, 1, 1), new DateTime(2018, 3, 10));
        }

        [TestMethod]
        public void not_continuous_multiple_budget_month()
        {
            GivenBudgets(new Budget() { YearMonth = "201801", Amount = 62 },
                new Budget() { YearMonth = "201803", Amount = 62 });
            TotalAmountShouldBe(82, new DateTime(2018, 1, 1), new DateTime(2018, 3, 10));
        }

        [TestMethod]
        public void period_cross_year_when_multiple_budgets()
        {
            GivenBudgets(new Budget() { YearMonth = "201712", Amount = 310 },
                new Budget() { YearMonth = "201801", Amount = 310 },
                new Budget() { YearMonth = "201802", Amount = 280 },
                new Budget() { YearMonth = "201803", Amount = 310 });
            TotalAmountShouldBe(1000, new DateTime(2017, 12, 1), new DateTime(2018, 3, 10));
        }

        private void TotalAmountShouldBe(int expected, DateTime start, DateTime end)
        {
            var actual = _accounting.TotalAmount(start, end);

            actual.Should().Be(expected);
        }

        private void GivenBudgets(params Budget[] budgets)
        {
            _repository.GetAll().Returns(budgets.ToList());
        }
    }
}