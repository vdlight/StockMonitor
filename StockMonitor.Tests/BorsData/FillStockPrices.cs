using NUnit.Framework;

using StocksMonitor.BorsData.BorsdataNS;
using StocksMonitor.Data.DataStoreNS;

namespace StocksMonitor.Tests.BorsData_NS
{
    [TestFixture]
    public class FillStockPrices
    {
        private DataStore _store;
        private BD _bd;

        [SetUp]
        public void Setup()
        {
            _bd = new BD();
            _store = new DataStore(_bd);
        }

        [Test]
        public void Run_CheckConstructionOfStockData()
        {
            _bd.Run();
            _store.FillStoreFromBD();


            Assert.That(_store.stocks[0].Name, Is.EqualTo("SAAB"));
            Assert.That(_store.stocks[0].List, Is.EqualTo("Large Cap"));
            Assert.That(Math.Round(
                _store.stocks[0].Price, 2), Is.EqualTo(Math.Round((decimal)90, 2))); // latest value

            Assert.That(Math.Round(
                _store.stocks[0].History[0].Price, 2), Is.EqualTo(Math.Round((decimal)100, 2)));
            Assert.That(
                _store.stocks[0].History[0].Date, Is.EqualTo(DateTime.Parse("2024-09-03")));
            Assert.That(Math.Round(
                _store.stocks[0].History[1].Price, 2), Is.EqualTo(Math.Round((decimal)90, 2)));
            Assert.That(
                _store.stocks[0].History[1].Date, Is.EqualTo(DateTime.Parse("2024-10-02")));


            Assert.That(_store.stocks[1].Name, Is.EqualTo("ABB"));
            Assert.That(_store.stocks[1].List, Is.EqualTo("Mid Cap"));
            Assert.That(Math.Round(
                _store.stocks[1].Price, 2), Is.EqualTo(Math.Round((decimal)120, 2))); // latest value

            Assert.That(Math.Round(
                _store.stocks[1].History[0].Price, 2), Is.EqualTo(Math.Round((decimal)80, 2)));
            Assert.That(
                _store.stocks[1].History[0].Date, Is.EqualTo(DateTime.Parse("2024-09-03")));
            Assert.That(Math.Round(
                _store.stocks[1].History[1].Price, 2), Is.EqualTo(Math.Round((decimal)120, 2)));
            Assert.That(
                _store.stocks[1].History[1].Date, Is.EqualTo(DateTime.Parse("2024-10-02")));
        }
    }
}