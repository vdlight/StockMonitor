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
using GrapeCity.DataVisualization.TypeScript;
using GrapeCity.DataVisualization.Chart;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace StocksMonitor.src.dataStoreNS
{
    public class DataStore
    {
        public List<Stock> stocks = new List<Stock>();
        private AvanzaParser avanza;
        private Storage storage = new Storage();
        private BorsData bd;


        private string[] Markets = 
        {
            "First North Stockholm",
            "Small Cap Stockholm",
            "Mid Cap Stockholm",
            "Large Cap Stockholm",
            "Own",
        };

        public DataStore(AvanzaParser avanza)
        {
            this.avanza = avanza;
        }

        public DataStore(BorsData bd)
        {
            this.bd = bd;
        }
        public DataStore() 
        { 
            this.bd = new BorsData();
            this.avanza = new AvanzaParser();
        }

        private void MarkIndexes()
        {
            foreach (var item in stocks)
            {
                item.IsIndex = false;
            }

            foreach (var item in stocks.Where(s => bd.indexes.Contains(s.Name)))
            {
                item.IsIndex = true;
            }
        }

        public async void Startup()
        {
            stocks = await storage.ReadData();
            MarkIndexes();
        }
     
        public async void GetOwnedData()
        {
            var ownedStocks = avanza.Run();

            stocks.ForEach(s =>
            {
                s.OwnedCnt = 0;
                s.PurPrice = 0;
            });

            foreach (var stock in ownedStocks)
            {
                var match = stocks.Find(s => s.Name == stock.Name && s.Price == stock.Price);

                if (match != null)
                {
                    match.OwnedCnt = stock.OwnedCnt;
                    match.PurPrice = stock.PurPrice;
                }
            }

            var stocksNotMatched = ownedStocks.FindAll(o => !stocks.Any(s => o.Name == s.Name && o.Price == s.Price));

            foreach (var stock in stocksNotMatched)
            {
                StockMonitorLogger.WriteMsg("ERROR: Could not connect owned stock with name " + stock.Name
                    + " and price " + stock.PurPrice);
            }

            WriteToDb();
        }



        private void UpdateStockHistory(History history, List<StockPriceV1> prices)
        {
            history.Price = (decimal)prices[0].C;
            history.MA200 = bd.CalculateMa200Percentage(prices);
        }

        private void UpdateStock(Stock stock, List<StockPriceV1> prices, decimal PE, decimal divident)
        {

            var newestValueFirstList = prices.ToList();
            newestValueFirstList.Reverse();

            stock.Price = (decimal)newestValueFirstList[0].C;
            stock.MA200 = bd.CalculateMa200Percentage(newestValueFirstList);
            stock.PeValue = PE;
            stock.Divident = divident;

            while (newestValueFirstList.Any())
            {
                var priceDate = DateTime.Parse(newestValueFirstList[0].D);
                
                if (! stock.History.Any(h => h.Date == priceDate))
                { 
                    var newHistory = new History();
                    newHistory.Date = DateTime.Parse(newestValueFirstList[0].D);
                    UpdateStockHistory(newHistory, newestValueFirstList);
                    stock.History.Add(newHistory);
                }

                newestValueFirstList.RemoveAt(0);
            }
        }

        public void UpdateStockDataBD()
        {
            bd.Run();
            
            FillStoreFromBD();
            WriteToDb();

        }
        public void FillStoreFromBD()
        {
            foreach (var instrument in bd.InstrumentDatas)
            {
                if (stocks.Any(s => s.Name == instrument.Key))
                {
                    UpdateStock(
                        stocks.First(s => s.Name == instrument.Key),
                        prices: instrument.Value.prices,
                        PE: instrument.Value.PE,
                        divident: instrument.Value.Divident
                    );
                }
                else
                {
                    var newStock = new Stock();
                    newStock.Name = instrument.Key;

                    var marketName = bd.GetMarketName(newStock.Name);
                    if (marketName != "") 
                    {
                        newStock.List = marketName;
                        UpdateStock(newStock, 
                            prices: instrument.Value.prices, 
                            PE: instrument.Value.PE, 
                            divident: instrument.Value.Divident);
                        
                        stocks.Add(newStock);
                    }
                }
            }
            MarkIndexes();
        }
        public async Task WriteToDb()
        {
            await storage.WriteData(stocks);
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


#if DEBUG
        public void ClearStocks()
        {
            stocks = new();
        }

#endif

    }
}
