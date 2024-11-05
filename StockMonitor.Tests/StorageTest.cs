using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using StocksMonitor.src;
using StocksMonitor.src.databaseWrapper;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Tests.StorageNs
{
    [TestFixture]
    public class StorageTest
    {
        private Storage storage;
        private List<Stock> stocks;
        private const string connStr = "Server=JENSA;Database=master;Integrated Security=True;TrustServerCertificate=True;";

        [SetUp]
        public void Setup()
        {
            storage = new Storage();

            ClearAllTablesInDB();
        }

        [Test]
        public async Task ClearDataAtStartup()
        {
            stocks = await storage.ReadData(connStr);

            Assert.That(stocks.Count, Is.EqualTo(0));

        }

        [Test]
        public async Task WriteData_ShallWriteAllStocks_WhenNew()
        {
            stocks = new List<Stock> {
                new Stock()
                {
                    Name = "AAK",
                    List = "First North",
                    MA200 = 0,
                    Divident = 1,
                    PeValue = -20,
                    OwnedCnt = 0,
                    Price = 200,
                    PurPrice = 150,
                    History = new List<History>
                    {
                        new History
                        {
                            MA200 = 0,
                            Price = 200,
                            Date = DateTime.Parse("2024-08-02"),
                        },
                        new History
                        {
                            MA200 = 22,
                            Price = 230,
                            Date = DateTime.Parse("2024-09-04"),
                        },
                        new History
                        {
                            MA200 = 20,
                            Price = 220,
                            Date = DateTime.Parse("2024-10-06"),
                        }
                    }
                },
                new Stock()
                {
                    Name = "SAAB",
                    List = "Large Cap",
                    MA200 = 23,
                    Divident = 4,
                    PeValue = 20,
                    OwnedCnt = 3,
                    Price = 2022,
                    PurPrice = 1323,
                    History = new List<History>
                    {
                        new History
                        {
                            MA200 = -23,
                            Price = 20,
                            Date = DateTime.Parse("2024-08-01"),
                        },
                        new History
                        {
                            MA200 = 22,
                            Price = 230,
                            Date = DateTime.Parse("2024-09-03"),
                        },
                        new History
                        {
                            MA200 = 20,
                            Price = 220,
                            Date = DateTime.Parse("2024-10-04"),
                        }
                    }
                }
            };


            await storage.WriteData(stocks, connStr);

            var readStocks = await storage.ReadData(connStr);

            AssertData(readStocks, stocks);

            stocks = readStocks;

            stocks.Add(
                new Stock
                {
                    Name = "SEB",
                    List = "Mid Cap",
                    MA200 = 32,
                    Divident = 14,
                    PeValue = 0,
                    OwnedCnt = 44,
                    Price = 20,
                    PurPrice = 10,
                    History = new List<History>
                    {
                        new History
                        {
                            MA200 = 22,
                            Price = 20,
                            Date = DateTime.Parse("2024-07-02"),
                        },
                        new History
                        {
                            MA200 = 222,
                            Price = 2303,
                            Date = DateTime.Parse("2024-08-04"),
                        },
                        new History
                        {
                            MA200 = 10,
                            Price = 2220,
                            Date = DateTime.Parse("2024-10-06"),
                        }
                    }
                });

            stocks[0].History.Add(
            new History
            {
                Date = DateTime.Parse("2024-11-23"),
                MA200 = 22,
                Price = 2242,
            });
         
            // After adding another stock with three history, it shall be there
            // also updating a stock with new data, that info shall also be there.
            await storage.WriteData(stocks, connStr);

            readStocks = await storage.ReadData(connStr);
            AssertData(readStocks, stocks);

        }

        private void AssertData(List<Stock> actual, List<Stock> expected)
        {
            Assert.That(actual.Count, Is.EqualTo(expected.Count));

            for (var i = 0; i < actual.Count; i++)
            {
                CompareStockValues(actual[i], expected[i]);
                Assert.That(actual[i].History.Count, Is.EqualTo(expected[i].History.Count));

                for (var j = 0; j < actual[i].History.Count; j++)
                {
                    CompareHistoryValues(actual[i].History[j], expected[i].History[j]);
                }
            }
        }
        private void CompareStockValues(Stock actual, Stock expected)
        {
            const int decimals = 2;
            Assert.That(actual.Name , Is.EqualTo(expected.Name));

            Assert.That(actual.List, Is.EqualTo(expected.List));

            Assert.That(actual.OwnedCnt, Is.EqualTo(expected.OwnedCnt));

            Assert.That(Math.Round(actual.Price, decimals), Is.EqualTo(Math.Round(expected.Price, decimals)));

            Assert.That(Math.Round(actual.MA200, decimals), Is.EqualTo(Math.Round(expected.MA200, decimals)));

            Assert.That(Math.Round(actual.Divident, decimals), Is.EqualTo(Math.Round(expected.Divident, decimals)));

            Assert.That(Math.Round(actual.PeValue, decimals), Is.EqualTo(Math.Round(expected.PeValue, decimals)));

            Assert.That(Math.Round(actual.PurPrice, decimals), Is.EqualTo(Math.Round(expected.PurPrice, decimals)));
        }

        private void CompareHistoryValues(History actual, History expected)
        {
            const int decimals = 2;

            
            Assert.That(actual.Date, Is.EqualTo(expected.Date));

            Assert.That(Math.Round(actual.Price, decimals), Is.EqualTo(Math.Round(expected.Price, decimals)));

            Assert.That(Math.Round(actual.MA200, decimals), Is.EqualTo(Math.Round(expected.MA200, decimals)));
        }
        private async Task ClearAllTablesInDB()
        {
            await using (var db = new StockDataContext(connStr))
            {
                var query =
                    "DELETE FROM history" + "\n" +
                    "DELETE FROM stockData" + "\n" +
                    "DELETE FROM miscs";

                await db.Database.ExecuteSqlRawAsync(query);

            }
        }
    }
}
