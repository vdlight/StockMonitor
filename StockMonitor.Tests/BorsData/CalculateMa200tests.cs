using Borsdata.Api.Dal.Model;
using NUnit.Framework;
using StocksMonitor.src.Borsdata;



namespace StockMonitor.Tests.BorsData_NS
{
    [TestFixture]
    public class CalculateMa200
    {
        private BorsData _bd;

        [SetUp]
        public void Setup()
        {
            _bd = new BorsData();
        }

        [Test]
        public void CalculateMa200Percentage_LessThan200Values_ReturnsZero()
        {
            var cnt = 198;
            var values = Enumerable.Range(0, cnt).Select(i => new StockPriceV1 { C = 100 }).ToList();
            values[cnt-1] = new StockPriceV1 { C = 313 };  

            var result = _bd.CalculateMa200Percentage(values);

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void CalculateMa200Percentage_Exactly200Values_ReturnsExpectedPercentage()
        {
            var cnt = 200;
            var values = Enumerable.Range(1, cnt).Select(i => new StockPriceV1 { C = 285.18 }).ToList();
            values[cnt-1] = new StockPriceV1 { C = 313 };  // Latest value is 80, others are 100

            var result = _bd.CalculateMa200Percentage(values);

            Assert.That(Math.Round(result, 2), Is.EqualTo(9.70m));
        }

        [Test]
        public void CalculateMa200Percentage_LatestValueGreaterThanMa200_ReturnsPositivePercentage()
        {
            var cnt = 300;
            var values = Enumerable.Range(1, cnt).Select(i => new StockPriceV1 { C = 285.18 }).ToList();
            values[cnt-1] = new StockPriceV1 { C = 313 };  // Latest value is 80, others are 100

            var result = _bd.CalculateMa200Percentage(values);

            Assert.That(Math.Round(result, 2), Is.EqualTo(9.70m));
        }
        [Test]
        public void CalculateMa200Percentage_LatestValueLessThanMa200_ReturnsNegativePercentage()
        {
            var cnt = 300;
            var values = Enumerable.Range(1, cnt).Select(i => new StockPriceV1 { C = 180.99 }).ToList();
            values[cnt-1] = new StockPriceV1 { C = 148.83};  

            var result = _bd.CalculateMa200Percentage(values);

            Assert.That(Math.Round(result, 2), Is.EqualTo(-17.7m));
        }
        [Test]
        public void CalculateMa200Percentage_VariedValues_ReturnsExpectedPercentage()
        {
            var values = new List<StockPriceV1>();
            values.Add(new StockPriceV1 { C = 109 });
            values.Add(new StockPriceV1 { C = 108 });
            values.Add(new StockPriceV1 { C = 107 });
            values.Add(new StockPriceV1 { C = 106 });
            values.Add(new StockPriceV1 { C = 105 });
            for (int i = 0; i < 200; i++)
            {
                values.Add(new StockPriceV1 { C = 100 + (i % 10) });
            }


            var result = _bd.CalculateMa200Percentage(values);

            Assert.That(Math.Round(result, 2), Is.EqualTo(4.31));
        }
    }
}

