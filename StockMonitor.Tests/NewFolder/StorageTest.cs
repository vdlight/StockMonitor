using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using StocksMonitor.src;
using StocksMonitor.src.databaseWrapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Tests.StorageTests_NS
{
    [TestFixture]
    public class StorageTest
    {
        private Storage storage;
        private List<Stock> stocks;
        private const string connStr = "Server=NEX-5CD350FDG5;Database=Test;Integrated Security=True;TrustServerCertificate=True;";

        [OneTimeSetUp]
        public void Setup()
        {
            storage = new Storage();

            ClearAllTablesInDB();
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
                            OwnedCnt = 20,
                            Date = DateTime.Parse("2024-08-02"),
                        },
                        new History
                        {
                            MA200 = 22,
                            Price = 230,
                            OwnedCnt = 30,
                            Date = DateTime.Parse("2024-09-04"),
                        },
                        new History
                        {
                            MA200 = 20,
                            Price = 220,
                            OwnedCnt = 24,
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
                            OwnedCnt = 20,
                            Date = DateTime.Parse("2024-08-01"),
                        },
                        new History
                        {
                            MA200 = 22,
                            Price = 230,
                            OwnedCnt = 30,
                            Date = DateTime.Parse("2024-09-03"),
                        },
                        new History
                        {
                            MA200 = 20,
                            Price = 220,
                            OwnedCnt = 24,
                            Date = DateTime.Parse("2024-10-04"),
                        }
                    }
                }
            };

            await storage.WriteData(stocks, connStr);

            var readStocks = await storage.ReadData(connStr);

            AssertData(readStocks, stocks);

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
                            OwnedCnt = 0,
                            Date = DateTime.Parse("2024-07-02"),
                        },
                        new History
                        {
                            MA200 = 222,
                            Price = 2303,
                            OwnedCnt = 3,
                            Date = DateTime.Parse("2024-08-04"),
                        },
                        new History
                        {
                            MA200 = 10,
                            Price = 2220,
                            OwnedCnt = 14,
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
                OwnedCnt = 1

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
                Assert.That(CompareStockValues(actual[i], expected[i]), Is.True);
                Assert.That(actual[i].History.Count, Is.EqualTo(expected[i].History.Count));

                for (var j = 0; j < actual[i].History.Count; j++)
                {

                    Assert.That(CompareHistoryValues(actual[i].History[j], expected[i].History[j]), Is.True);
                }
            }
        }

        private bool cmpDec(decimal actual, decimal expected)
        {
            if (Math.Round(actual, 2) != Math.Round(expected, 2))
            {
                return false;
            }
            return true;
        }
        private bool CompareStockValues(Stock actual, Stock expected)
        {
            if (actual.Name != expected.Name)
                return false;

            if (actual.List != expected.List)
                return false;

            if (actual.OwnedCnt != expected.OwnedCnt)
                return false;

            if (!cmpDec(actual.Price, expected.Price))
                return false;

            if (!cmpDec(actual.MA200, expected.MA200))
                return false;

            if (!cmpDec(actual.Divident, expected.Divident))
                return false;

            if (!cmpDec(actual.PeValue, expected.PeValue))
                return false;

            if (!cmpDec(actual.PurPrice, expected.PurPrice))
                return false;

            return true;
        }

        private bool CompareHistoryValues(History actual, History expected)
        {
            if(actual.OwnedCnt != expected.OwnedCnt)
                return false;
            if (actual.Date != expected.Date)
                return false;

            if (!cmpDec(actual.Price, expected.Price))
                return false;

            if (!cmpDec(actual.MA200, expected.MA200))
                return false;

            return true;
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
