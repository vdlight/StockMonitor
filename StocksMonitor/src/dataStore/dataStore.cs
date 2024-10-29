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

        public async void Startup()
        {
            stocks = await storage.ReadData();
        }

     
        public void GetOwnedData()
        {

        }

        public void FillOwnedData()
        {

        }
        private void UpdateStockHistory(History history, List<StockPriceV1> prices)
        {
            history.Price = (decimal)prices[0].C;
            history.MA200 = bd.CalculateMa200Percentage(prices);
        }

        private void UpdateStock(Stock stock, List<StockPriceV1> prices)
        {
            stock.Price = (decimal) prices[0].C;
            stock.MA200 = bd.CalculateMa200Percentage(prices);

            while (prices.Any())
            {
                var priceDate = DateTime.Parse(prices[0].D);
                
                if (! stock.History.Any(h => h.Date == priceDate))
                { 
                    var newHistory = new History();
                    newHistory.Date = DateTime.Parse(prices[0].D);
                    UpdateStockHistory(newHistory, prices);
                    stock.History.Add(newHistory);
                }
                
                prices.RemoveAt(0);
            }
        }

        public void UpdateStockDataBD()
        {
            bd.Run();
            
            FillStore();
            WriteToDb();

        }
        public void FillStore()
        {
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
                    newStock.Name = instrument.Key;

                    var marketName = bd.GetMarketName(newStock.Name);
                    if (marketName != "") 
                    {
                        newStock.List = marketName;
                        UpdateStock(newStock, instrument.Value);
                        stocks.Add(newStock);
                    }
                }
            }
        }
        public async Task WriteToDb()
        {
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


#if DEBUG
        public void ClearStocks()
        {
            stocks = new();
        }

#endif

    }
}
