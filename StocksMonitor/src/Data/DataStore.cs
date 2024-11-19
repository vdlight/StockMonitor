
using Borsdata.Api.Dal.Model;

using GrapeCity.DataVisualization.TypeScript;

using StocksMonitor.Avanza.AvanzaParserNS;
using StocksMonitor.BorsData.BorsdataNS;
using StocksMonitor.Data.HistoryNS;
using StocksMonitor.LoggerNS;
using StocksMonitor.Data.StockNS;
using StocksMonitor.Storage.DatabaseNS;


namespace StocksMonitor.Data.DataStoreNS
{
    public class DataStore
    {
        public List<Stock> stocks = new List<Stock>();
        private AvanzaParser avanza;
        private Database storage = new Database();
        private BD bd;
        
        private Dictionary<string, string> AvanzaToBD = new Dictionary<string, string>
        { 
            { "Samhällsbyggnadsbo. i Norden B", "Samhällsbyggnadsbolag B" },
            { "Platzer Fastigheter Holding B", "Platzer Fastigheter" },
//            { "Proact IT Group", "Proact IT" },
  //          { "Lundin Mining Corporation", "Lundin Mining" },
//            { "Green Landscaping Group", "Green Landscaping"},
            {"Fast. Balder B", "Fast Balder" },
            {"KlaraBo Sverige B", "KlaraBo" },
  //          {"Genova Property Group","Genova Property"},
    //        {"Nederman Holding", "Nederman"},       // TODO, ignore Holding eller Group på slutet
      //      {"Railcare Group", "Railcare" },
            {"Sampo Oyj SDB", "Sampo" },
            {"Byggmästare A J Ahlström H", "Byggmästare AJ Ahlström"},
            {"Arion Banki SDB","Arion Banki" },
            {"John Mattson Fastighetsföret.", "John Mattson" },
        //    {"Karnov Group", "Karnov" },
          //  {"Norva24 Group", "Norva24" },
         //   {"Scandic Hotels Group", "Scandic Hotels" },
            {"Millicom Int. Cellular SDB", "Millicom" },
          //  {"Byggmax Group", "Byggmax" },
            {"Lagercrantz Group B", "Lagercrantz"},
            {"Embracer Group B", "Embracer" },
          //  {"BONESUPPORT HOLDING", "Bonesupport" },
          //  {"Alimak Group", "Alimak" },
            {"Cibus Nordic Real Estate", "Cibus Nordic"},
            {"Balder B", "Fast Balder"},
            {"Arion Bank SDB", "Arion Banki"},
            {"Byggmästare Anders J Ahlström Holding B", "Byggmästare AJ Ahlström"},
            {"Emilshus B", "Fastighetsbolaget Emilshus"},
            {"Nivika", "Nivika Fastigheter" },
            {"Sampo A SDB", "Sampo"},
            {"TRATON SE", "Traton" }
        };

        public DataStore(AvanzaParser avanza)
        {
            this.avanza = avanza;

        }

        public DataStore(BD bd)
        {
            this.bd = bd;
        }
        public DataStore() 

        { 
            // TODO: rita ut i grafen på simuleringar, så jag kan se sälj / köp ,, justering i grafen, samt värdet under hela perioden
            // detta då när man simulerar en (eller fåtal) aktie bara. 
            this.bd = new BD();
            this.avanza = new AvanzaParser();
        }
        // TODOD, testa att bara köra vinster vid 20 % och sälja, szedan börja om, snitta ner vid behov
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
                
                var match = stocks.Find(s => s.Name.ToLower() == stock.Name.ToLower()); // direct match by name

                if (match == null)
                {
                    try
                    {
                        match = ConvertedNameSearch(stock.Name, stock.Price); // match with converted name and price match
                    }
                    catch (Exception ex)
                    {
                        StocksMonitorLogger.WriteMsg("ERROR, did not find a match for " + stock.Name);
                    }
                }

                if (match != null)
                {
                    match.OwnedCnt = stock.OwnedCnt;
                    match.PurPrice = stock.PurPrice;
                }
            }

            await WriteToDb();
        }

        private Stock ConvertedNameSearch(string avanzaName, decimal price)
        {
            // OFten BD ignore stating A or B stock, when there is only one. And filter away caps to find match
            var match = stocks.find(stocks => stocks.Name.ToLower() == avanzaName.substr(0, avanzaName.Length - 1).trim().ToLower());

            if (match != null)
            {
                return match;
            }

            var avanzaWords = avanzaName.split(" ");

            avanzaWords.RemoveAll(str => str.ToLower() == "group" || str.ToLower() == "holding" || str.ToLower() == "Corporation");
            avanzaName = avanzaWords.join(" ");

            match = stocks.find(stocks => stocks.Name.ToLower() == avanzaName.substr(0, avanzaName.Length - 1).trim().ToLower());

            if (match != null)
            {
                return match;
            }

            var convertedName = AvanzaToBD[avanzaName].ToLower();
            match = stocks.find(stocks => stocks.Name.ToLower() == convertedName);

            if (match != null)
            {
                if (match.Price == price)
                    return match;

            }
            
            return null;
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
