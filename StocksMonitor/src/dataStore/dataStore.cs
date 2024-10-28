using StocksMonitor.src.databaseWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StocksMonitor.src.avanzaParser;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Identity.Client;
using StocksMonitor.src.Borsdata;
using Borsdata.Api.Dal.Model;
using System.Net.Http.Headers;

namespace StocksMonitor.src.dataStore
{
    public class DataStore
    {
        public List<Stock> stocks = new List<Stock>();
        private AvanzaParser avanza = new AvanzaParser();
        private Storage storage = new Storage();
        private BorsData bd = new BorsData();
        private string[] Markets = 
        {
            "First North Stockholm",
            "Small Cap Stockholm",
            "Mid Cap Stockholm",
            "Large Cap Stockholm",
            "Own",
        };


        public async void Startup()
        {
            stocks = await storage.ReadData();
        }

     
        public void GetOwnedData()
        {

        }

        private void UpdateStockHistory(History history, List<StockPriceV1> prices)
        {
            history.Price = (decimal)prices[0].C;
            history.MA200 = bd.CalculateMa200Percentage(prices);
            history.Date = DateTime.Parse(prices[0].D);
        }

        private void UpdateStock(Stock stock, List<StockPriceV1> prices)
        {
            stock.Price = (decimal) prices[0].C;
            stock.MA200 = bd.CalculateMa200Percentage(prices);

            while (prices.Any())
            {
                var priceDate = DateTime.Parse(prices[0].D);
                
                if (stock.History.Any(h => h.Date == priceDate))
                { 
                    UpdateStockHistory(
                        stock.History.Find(h => h.Date == priceDate), 
                        prices);
                }
                else
                {
                    var newHistory = new History();
                    UpdateStockHistory(newHistory, prices);
                    stock.History.Add(newHistory);
                }
                
                prices.RemoveAt(0);
            }
        }

        public async Task UpdateStockDataBD()
        {
            bd.Run();

            foreach (var instrument in bd.InstrumentPrices)
            {
                if (stocks.Any(s => s.Name == instrument.Key))
                {
                    UpdateStock(
                        stocks.First(s => s.Name == instrument.Key),
                        instrument.Value
                    );
                }
                else
                {
                    var newStock = new Stock();
                    UpdateStock(newStock, instrument.Value);
                    stocks.Add(newStock);
                }
            }
            await storage.WriteData(stocks);
        }


        public async Task FetchDataFromAvanza(DateTime date)
        {
            StockMonitorLogger.WriteMsg("Parse data from Avanza");
            
            // TODO: enable
            
            //var parsedData = avanza.Run();
            /*
            // TODO, add more feasability checks
            if (parsedData != null && parsedData.Count > 10)
            {
                await UpdateStoreWithNewStocks(parsedData, date);
            }
            else
            {
                StockMonitorLogger.WriteMsg("TOOD ERROR");
            }*/
        }

        private async Task CalculateHistorySums()
        {
            Dictionary<string, List<decimal>> meanValues = [];

            foreach(var stock in stocks)
            {
                foreach(var history in stock.History)
                {
                    if (stock.OwnedCnt > 0)
                    {
                        meanValues["Own"].Add(stock.Price);
                    }

                    meanValues[stock.List].Add(stock.Price);
                }
                if (stock.OwnedCnt > 0)
                {
                    meanValues["Own"].Add(stock.Price);
                }

                meanValues[stock.List].Add(stock.Price);
            }

            await Task.CompletedTask;
        }
        // TODO, När man markerar något i listan, så ska det visas procentutveckling i grafen, från hur det varit senaste, 1, 1w 1m 1y


        private void UpdateStock(Stock stock, DateTime date)
        {
            if(!stocks.Any(s => s.Name == stock.Name))
            {
                // new stock
                stock.History.Add(new ()
                {
                    Date = date,
                    MA200 = stock.MA200,
                    OwnedCnt = stock.OwnedCnt,
                    Price = stock.Price,    
                });

                stocks.Add(stock);
            }
            else
            {
                var existingStock = stocks.First(s => s.Name == stock.Name);

                existingStock.PurPrice = stock.PurPrice;
                existingStock.Price = stock.Price;
                existingStock.MA200 = stock.MA200;
                existingStock.OwnedCnt = stock.OwnedCnt;
                existingStock.List = stock.List;
                
                if(existingStock.History.Any(h => h.Date == date))
                {
                    var existingHistory = existingStock.History.First(h => h.Date == date);
                    existingHistory.MA200 = stock.MA200;
                    existingHistory.OwnedCnt = stock.OwnedCnt;  
                    existingHistory.Price = stock.Price;
                }
                else
                {
                    existingStock.History.Add(new()
                    {
                        MA200 = stock.MA200,
                        OwnedCnt = stock.OwnedCnt,
                        Price = stock.Price,
                        Date = date
                    });
                }
            }
        }        

#if DEBUG
        public void ClearStocks()
        {
            stocks = new();
        }


        /*
         * Creates three stocks, 
         * Push history on all stocks to make room for more
         * updates all three stock value
         * Push history on all stocks to make room for more
         * creates one additional stock.
         * 
         * Checks, stocks and history that it corresponds from db to local stored stocks
         */
        public async Task TestRun()
        {
            // Clear StockData, History and Miscs
            await storage.ClearDatabaseTables();

            // testdata
            var testInputStocks = new List<Stock>
            {
                new() {Name="ABB", MA200=55.2m, Price=525.2m, PurPrice=20, OwnedCnt=255},
                new() {Name="SAAB", MA200=33.2m, Price=44.2m, PurPrice=233, OwnedCnt=224},
                new() {Name="SBB", MA200=5.2m, Price=99m, PurPrice=43, OwnedCnt=22},
            };

         //   await UpdateStoreWithNewStocks(testInputStocks);
            await compareInputAndOutputData(testInputStocks);

            int daysOffset = -5;
            // Push old history five days 
            var today = DateTime.Now.Date;
            var pushDay = today.AddDays(daysOffset);

            foreach(Stock stock in testInputStocks)
            {
                stock.MA200 += 14;
                stock.Price += 14;
                stock.OwnedCnt += 14;

                foreach(History history in stock.History)
                {
                    if(history.Date == today)
                    {
                        history.Date = pushDay;
                    }
                }
            }
            await storage.UpdateHistoryDateFromTo(today.ToString(), pushDay.ToString());
            daysOffset++;

      //      await UpdateStoreWithNewStocks(testInputStocks);
            await compareInputAndOutputData(testInputStocks);

            pushDay = today.AddDays(daysOffset++);

            await storage.UpdateHistoryDateFromTo(today.ToString(), pushDay.ToString());

            foreach (Stock stock in testInputStocks)
            {
                stock.MA200 += 14;
                stock.Price += 14;
                stock.OwnedCnt += 14;

                foreach (History history in stock.History)
                {
                    if (history.Date == today)
                    {
                        history.Date = pushDay;
                    }
                }
            }
            await storage.UpdateHistoryDateFromTo(today.ToString(), pushDay.ToString());
            daysOffset++;

            testInputStocks.Add(new ()
            {
                Name = "Systembolaget",
                MA200 = -14,
                Price = 2.4m,
                PurPrice = -2.3m,
                OwnedCnt = 2
            });

           // await //UpdateStoreWithNewStocks(testInputStocks);

            await compareInputAndOutputData(testInputStocks);

        }
        private async Task compareInputAndOutputData(List<Stock> testInputStocks)
        {
            var testOutputStocks = await storage.ReadData();


            for (var i = 0; i < testInputStocks.Count; i++)
            {
                var inputStock = testInputStocks[i];
                var outputStock = testOutputStocks[i];

                checkData(inputStock.Price, outputStock.Price);
                checkData(inputStock.OwnedCnt, outputStock.OwnedCnt);
                checkData(inputStock.MA200, outputStock.MA200);
                checkData(inputStock.PurPrice, outputStock.PurPrice);

                if (inputStock.History == null)
                {
                    checkData(outputStock.History, null);
                }
                else
                {
                    for (var j = 0; j < inputStock.History.Count; j++)
                    {
                        var inputHistory = inputStock.History.ToList()[j];
                        var outputHistory = outputStock.History.ToList()[j];

                        checkData(inputHistory.Price, outputHistory.Price);
                        checkData(inputHistory.OwnedCnt, outputHistory.OwnedCnt);
                        checkData(inputHistory.MA200, outputHistory.MA200);
                        checkData(inputHistory.Date, outputHistory.Date);
                        checkData(outputStock.ID, outputHistory.StockId);
                    }
                }
            }

        }

        private void checkData(int lhs, int rhs)
        {
            if (lhs != rhs)
            {
                StockMonitorLogger.WriteMsg("Error");
            }
        }
        private void checkData(decimal lhs, decimal rhs)
        {
            if (lhs != rhs)
            {
                StockMonitorLogger.WriteMsg("Error");
            }
        }

        private void checkData(string lhs, string rhs)
        {
            if (lhs != rhs)
            {
                StockMonitorLogger.WriteMsg("Error");
            }
        }
        private void checkData(DateTime lhs, DateTime rhs)
        {
            if (lhs != rhs)
            {
                StockMonitorLogger.WriteMsg("Error");
            }
        }
        private void checkData(ICollection<History> lhs, ICollection<History>? rhs)
        {
            if (lhs != rhs)
            {
                StockMonitorLogger.WriteMsg("Error");
            }
        }
#endif

    }
}
