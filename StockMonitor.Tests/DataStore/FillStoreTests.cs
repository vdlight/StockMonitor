using Borsdata.Api.Dal.Model;
using NUnit.Framework;
using StocksMonitor.src.Borsdata;
using StocksMonitor.src.databaseWrapper;
using StocksMonitor.src.dataStoreNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Test.DataStoreNS.FillStoreNS
{
    public class DataStoreTests
    {
        private BorsData bd;
        private DataStore store;


        //[OneTimeSetUp]
        
        [SetUp]
        public void Setup()
        {
            bd = new BorsData();
            store = new DataStore(bd);
        }

        [Test]
        public void FillStore_ShouldAddNewStock_WhenInstrumentIsNew()
        {
    /*        bd.InstrumentPrices = new Dictionary<string, List<StockPriceV1>>
            {
            { "AAPL", new List<StockPriceV1> { new StockPriceV1 { C = 150, D = "2024-01-01" } } }
            };


            bd.GetAllMarkets();
            bd.GetAllInstruments();

            store.FillStoreFromBD();

            // Stock created
            Assert.That(store.stocks.Count, Is.EqualTo(1));
            Assert.That(store.stocks[0].Name, Is.EqualTo("AAPL"));
            Assert.That(store.stocks[0].Price, Is.EqualTo(150));

            // history created
            Assert.That(store.stocks[0].History.Count, Is.EqualTo(1));
            Assert.That(store.stocks[0].History[0].Date, Is.EqualTo(DateTime.Parse("2024-01-01")));
            Assert.That(store.stocks[0].History[0].Price, Is.EqualTo(150));
            Assert.That(store.stocks[0].History[0].MA200, Is.EqualTo(0));*/
        }

        [Test]
        public void FillStore_ShouldAddHistory_OnlyWhenHistoryIsNew()
        {
            store.stocks.Add(new Stock()
            {
                Name = "AAPL",
                Price = 150,
                MA200 = 0,
                History = new List<History>()
                {
                    new History() {
                        Price = 150,
                        MA200 = 0,
                        Date = new DateTime(2024,01,02)
                    }
                }
            });

            store.stocks.Add(new Stock()
            {
                Name = "SAAB",
                Price = 244,
                MA200 = 0,
            });

            /*bd.InstrumentPrices = new Dictionary<string, List<StockPriceV1>>
            {
            { store.stocks[0].Name, new List<StockPriceV1> { new StockPriceV1 { C = (double)store.stocks[0].Price, D = "2024-01-02" } } },
            { store.stocks[1].Name, new List<StockPriceV1> { new StockPriceV1 { C = (double)store.stocks[0].Price, D = "2024-02-27" } } },
            };
            */
            bd.GetAllMarkets();
            bd.GetAllInstruments();
            store.FillStoreFromBD();

            // Stocks created
            Assert.That(store.stocks.Count, Is.EqualTo(2));


            // histories created, only where needed
            Assert.That(store.stocks[0].History.Count, Is.EqualTo(1));
            Assert.That(store.stocks[0].History[0].Date, Is.EqualTo(DateTime.Parse("2024-01-02")));

            Assert.That(store.stocks[1].History.Count, Is.EqualTo(1));
            Assert.That(store.stocks[1].History[0].Date, Is.EqualTo(DateTime.Parse("2024-02-27")));
        }

        [Test]
        public void FillStore_ShouldNotModify_IfAllExists()
        {
            store.stocks.Add(new Stock()
            {
                Name = "AAPL",
                Price = 150,
                MA200 = 0,
                History = new List<History>()
                {
                    new History() {
                        Price = 150,
                        MA200 = 0,
                        Date = new DateTime(2024,01,02)
                    }
                }
            });

            store.stocks.Add(new Stock()
            {
                Name = "SAAB",
                Price = 244,
                MA200 = 0,
                History = new List<History>()
                {
                    new History() {
                        Price = 244,
                        MA200 = 0,
                        Date = new DateTime(2024,02,27)
                    }
                }
            });

            /*bd.InstrumentPrices = new Dictionary<string, List<StockPriceV1>>
            {
            { store.stocks[0].Name, new List<StockPriceV1> { new StockPriceV1 { C = (double)store.stocks[0].Price, D = "2024-01-02" } } },
            { store.stocks[1].Name, new List<StockPriceV1> { new StockPriceV1 { C = (double)store.stocks[0].Price, D = "2024-02-27" } } },
            };
            */
            bd.GetAllMarkets();
            bd.GetAllInstruments();

            store.FillStoreFromBD();

            // Stocks created
            Assert.That(store.stocks.Count, Is.EqualTo(2));

            // histories created, only where needed
            Assert.That(store.stocks[0].History.Count, Is.EqualTo(1));
            Assert.That(store.stocks[0].History[0].Date, Is.EqualTo(DateTime.Parse("2024-01-02")));

            Assert.That(store.stocks[1].History.Count, Is.EqualTo(1));
            Assert.That(store.stocks[1].History[0].Date, Is.EqualTo(DateTime.Parse("2024-02-27")));
        }

        [Test]
        public void FillStore_ShouldAddAll_IfNothingExists()
        {
            /*bd.InstrumentPrices = new Dictionary<string, List<StockPriceV1>>
            {
            { "SAAB", new List<StockPriceV1> {
                new StockPriceV1 { C = 200, D = "2024-01-02"},
                new StockPriceV1 { C = 210, D = "2024-02-22"}
            }},

            { "ABB", new List<StockPriceV1> {
                new StockPriceV1 { C = 421, D = "2024-01-27" },
                new StockPriceV1 { C = 400, D = "2024-02-27" }
            }}
            };
            */
            bd.GetAllMarkets();
            bd.GetAllInstruments();

            store.FillStoreFromBD();

            Assert.That(store.stocks.Count, Is.EqualTo(2));
            Assert.That(store.stocks[0].Name, Is.EqualTo("SAAB"));
            Assert.That(store.stocks[0].Price, Is.EqualTo(210));
            Assert.That(store.stocks[0].History.Count, Is.EqualTo(2));
            Assert.That(store.stocks[0].History[0].Date, Is.EqualTo(DateTime.Parse("2024-01-02")));
            Assert.That(store.stocks[0].History[1].Date, Is.EqualTo(DateTime.Parse("2024-02-22")));

            Assert.That(store.stocks[1].Name, Is.EqualTo("ABB"));
            Assert.That(store.stocks[1].Price, Is.EqualTo(400));
            Assert.That(store.stocks[1].History.Count, Is.EqualTo(2));
            Assert.That(store.stocks[1].History[0].Date, Is.EqualTo(DateTime.Parse("2024-01-27")));
            Assert.That(store.stocks[1].History[1].Date, Is.EqualTo(DateTime.Parse("2024-02-27")));
        }
    }
}
