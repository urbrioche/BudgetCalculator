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

        /// <summary>
        /// 資料庫沒預算
        /// </summary>
        [TestMethod]
        public void no_budgets()
        {
            GivenBudgets();
            TotalAmountShouldBe(0, new DateTime(2018, 3, 1), new DateTime(2018, 3, 1));
        }

        private void TotalAmountShouldBe(int expected, DateTime startDate, DateTime endDate)
        {
            Assert.AreEqual(expected, _accounting.TotalAmount(startDate, endDate));
        }

        private Accounting BudgetCalculat(List<Budget> budgets)
        {
            GivenBudgets(budgets.ToArray());

            _accounting = new Accounting(_repository);
            return _accounting;
        }

        private void GivenBudgets(params Budget[] budgets)
        {
            _repository.GetAll().Returns(budgets.ToList());
        }

        [TestMethod]
        public void 時間起訖不合法()
        {
            var target = BudgetCalculat(new List<Budget>());
            var start = new DateTime(2018, 3, 1);
            var end = new DateTime(2018, 2, 1);

            Action actual = () => target.TotalAmount(start, end);

            actual.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void 當一月預算為62_一月一號到一月三十一號_預算拿到62()
        {
            var target = BudgetCalculat(new List<Budget>() { new Budget() { YearMonth = "201801", Amount = 62 } });
            var start = new DateTime(2018, 1, 1);
            var end = new DateTime(2018, 1, 31);

            var actual = target.TotalAmount(start, end);

            actual.Should().Be(62);
        }

        [TestMethod]
        public void 當一月預算為62_一月一號到一月十五號_預算拿到30()
        {
            var target = BudgetCalculat(new List<Budget>() { new Budget() { YearMonth = "201801", Amount = 62 } });
            var start = new DateTime(2018, 1, 1);
            var end = new DateTime(2018, 1, 15);

            var actual = target.TotalAmount(start, end);

            actual.Should().Be(30);
        }

        [TestMethod]
        public void 找不到這個月的預算_拿到0()
        {
            var target = BudgetCalculat(new List<Budget>() { new Budget() { YearMonth = "201801", Amount = 62 } });
            var start = new DateTime(2018, 2, 1);
            var end = new DateTime(2018, 2, 15);

            var actual = target.TotalAmount(start, end);

            actual.Should().Be(0);
        }

        [TestMethod]
        public void 當一月預算為62_二月預算為280_一月一號到二月二十八號_預算拿到342()
        {
            var target = BudgetCalculat(new List<Budget>()
            {
                new Budget() { YearMonth = "201801", Amount = 62 },
                new Budget() { YearMonth = "201802", Amount = 280 }
            });
            var start = new DateTime(2018, 1, 1);
            var end = new DateTime(2018, 2, 28);

            var actual = target.TotalAmount(start, end);

            actual.Should().Be(342);
        }

        [TestMethod]
        public void 當一月預算為62_二月預算為280_三月預算為62_一月一號到三月十號_預算拿到362()
        {
            var target = BudgetCalculat(new List<Budget>()
            {
                new Budget() { YearMonth = "201801", Amount = 62 },
                new Budget() { YearMonth = "201802", Amount = 280 },
                new Budget() { YearMonth = "201803", Amount = 62 },
            });
            var start = new DateTime(2018, 1, 1);
            var end = new DateTime(2018, 3, 10);

            var actual = target.TotalAmount(start, end);

            actual.Should().Be(362);
        }

        [TestMethod]
        public void 當一月預算為62_二月預算為0_三月預算為62_一月一號到三月十號_預算拿到82()
        {
            var target = BudgetCalculat(new List<Budget>()
            {
                new Budget() { YearMonth = "201801", Amount = 62 },
                new Budget() { YearMonth = "201802", Amount = 0 },
                new Budget() { YearMonth = "201803", Amount = 62 },
            });
            var start = new DateTime(2018, 1, 1);
            var end = new DateTime(2018, 3, 10);

            var actual = target.TotalAmount(start, end);

            actual.Should().Be(82);
        }

        [TestMethod]
        public void 當十二月預算為310一月預算為310_二月預算為280_三月預算為310_十二月一號到三月十號_預算拿到1000()
        {
            var target = BudgetCalculat(new List<Budget>()
            {
                new Budget() { YearMonth = "201712", Amount = 310 },
                new Budget() { YearMonth = "201801", Amount = 310 },
                new Budget() { YearMonth = "201802", Amount = 280 },
                new Budget() { YearMonth = "201803", Amount = 310 },
            });
            var start = new DateTime(2017, 12, 1);
            var end = new DateTime(2018, 3, 10);

            var actual = target.TotalAmount(start, end);

            actual.Should().Be(1000);
        }
    }
}