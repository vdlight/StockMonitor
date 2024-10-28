using Borsdata.Api.Dal.Model;
using NUnit.Framework;

using StocksMonitor.src.Borsdata;



namespace Test.StockMonitor.tests
{
    [TestFixture]
    public class BorsDataTests
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
            
            var values = Enumerable.Range(0, 198).Select(i => new StockPriceV1 { C = 100 }).ToList();
            
            var result = _bd.CalculateMa200Percentage(values);
            
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void CalculateMa200Percentage_Exactly200Values_ReturnsExpectedPercentage()
        {
            var values = Enumerable.Range(0, 200).Select(i => new StockPriceV1 { C = 100 }).ToList();

            var result = _bd.CalculateMa200Percentage(values);

            // Sum of 200 * 100 / 200 = 100, so (100 - 100) / (100 * 100) = 0
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void CalculateMa200Percentage_LatestValueGreaterThanMa200_ReturnsPositivePercentage()
        {
            var values = Enumerable.Range(1, 200).Select(i => new StockPriceV1 { C = 100 }).ToList();
            values[0] = new StockPriceV1 { C = 120 };  // Latest value is 120, others are 100


            var result = _bd.CalculateMa200Percentage(values);

            // Expected: (120 - 100) / (100 * 100) = 0.002
            Assert.That(Math.Round(result, 3), Is.EqualTo(0.002m));
        }
        [Test]
        public void CalculateMa200Percentage_LatestValueLessThanMa200_ReturnsNegativePercentage()
        {
            var values = Enumerable.Range(1, 300).Select(i => new StockPriceV1 { C = 100 }).ToList();
            values[0] = new StockPriceV1 { C = 80 };  // Latest value is 80, others are 100

            var result = _bd.CalculateMa200Percentage(values);

            // Expected: (80 - 100) / (100 * 100) = -0.002
            Assert.That(Math.Round(result, 3), Is.EqualTo(-0.002m));
        }
        [Test]
        public void CalculateMa200Percentage_VariedValues_ReturnsExpectedPercentage()
        {
            var values = new List<StockPriceV1>();
            for (int i = 0; i < 205; i++)
            {
                values.Add(new StockPriceV1 { C = i + 100 });  // Incrementing values from 100 to 299
            }
            var result = _bd.CalculateMa200Percentage(values);

            // Sum = (100 + 299) * 200 / 2 = 39900; ma200 = 39900 / 200 = 199.5
            // Latest value = 100; expected: (100 - 199.5) / (199.5 * 100) ≈ -0.005
            Assert.That(Math.Round(result, 3), Is.EqualTo(-0.005m));
        }
    }
}

