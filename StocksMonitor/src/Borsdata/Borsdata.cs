using Borsdata.Api.Dal.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksMonitor.src.Borsdata
{
    public class BorsData
    {
        private List<InstrumentV1> _instruments;
        private BD_API _api;
        private Dictionary<string, long> _marketsId;

        const string largeCap = "Large Cap";
        const string midCap = "Mid Cap";
        const string smallCap = "Small Cap";
        const string firstNorth = "First North";

        public Dictionary<string, List<StockPriceV1>> InstrumentPrices { get; set; }

        public BorsData() 
        {
            InstrumentPrices = new Dictionary<string, List<StockPriceV1>>();
            _marketsId = new Dictionary<string, long>();    
           
        }

        public string GetMarketName(string instrumentName)
        {
# if DEBUG 
            return "Large Cap";

#endif

            var instrument = _instruments.Find(i => i.Name == instrumentName);
            
            if (instrument != null) {
                var match = _marketsId.Where(n => n.Value == instrument.MarketId).Select(n => n.Key).ToList();
                if (match.Count > 0)
                {
                    return match[0];
                }
            }

            StockMonitorLogger.WriteMsg("ERROR could not get market name, for stock with name " + instrumentName);

            return "";

        }

        public decimal CalculateMa200Percentage(IEnumerable<StockPriceV1> values)
        {
            if(values.Count() < 200)
            {
                return 0;
            }

            const int selectedMa = 200;
            var latestValue = values.First().C;
            var sum = values.Take(selectedMa).Sum(s => s.C);

            var ma200 = sum / selectedMa;
            var ma200Percent = (decimal)((latestValue - ma200) / (ma200 * 100));

            return ma200Percent;
        }


        public void Run()
        {
            var lines = File.ReadAllLines("..\\..\\..\\..\\..\\BDKey.txt");

            if (lines.Any())
            {
                _api = new BD_API(lines[0]);
                GetAllMarkets();
            

                GetAllInstruments();
                FillStockPrices();
            }
            else
            {
                StockMonitorLogger.WriteMsg("ERROR, could not fetch key");
            }

        }
        private void GetAllMarkets()
        {
            StockMonitorLogger.WriteMsg("Get all markets from BD");

            var _markets = _api.GetMarkets();
            if (_markets != null) {
                foreach (var market in _markets.Markets) {
                    if (market.Id.HasValue)
                    {
                        if(! _marketsId.ContainsKey(market.Name)) { 
                            _marketsId.Add(market.Name, market.Id.GetValueOrDefault());
                        }
                    }
                    else
                    {
                        StockMonitorLogger.WriteMsg("WARNING, could not read market id of " + market.Name);
                    }
                } 
            }
        }

        private void GetAllInstruments()
        {
            StockMonitorLogger.WriteMsg("Get all instruments from BD");
            _instruments = _api.GetInstruments().Instruments;
        }

        private void FillStockPrices()
        {
            StockMonitorLogger.WriteMsg("Fill stock prices from BD");

            FillInstrumentsFromMarket(_instruments.Where(i => i.MarketId == 
                _marketsId[largeCap]).ToList());

        /*  TODO, rest of markets  
        
            FillInstrumentsFromMarket(_instRespV1.Instruments.Where(i => i.MarketId == 
                _marketsId[midCap]).ToList());
            FillInstrumentsFromMarket(_instRespV1.Instruments.Where(i => i.MarketId ==
                _marketsId[smallCap]).ToList());
            FillInstrumentsFromMarket(_instRespV1.Instruments.Where(i => i.MarketId ==
                _marketsId[firstNorth]).ToList());*/

        }

        private void FillInstrumentsFromMarket(List<InstrumentV1> instruments)
        {
            // Get all stock prices for each instrument
            foreach (var i in instruments)
            {
                //--Get for a time range
                // TODO, läser just nu data för senaste året

                // TODO, kör första är äldstsa datum när jag läst in till histoiken
                StockPricesRespV1 sp = _api.GetStockPrices(i.InsId.Value, DateTime.Today.AddYears(-1), DateTime.Today);

                //StockPricesRespV1 sp = _api.GetStockPrices(i.InsId.Value);
                if (sp != null)
                {
                    try
                    {
                        var instrumentName = _instruments[(int)i.InsId.Value].Name;
                        InstrumentPrices[instrumentName] = sp.StockPricesList;
                    }
                    catch (Exception ex)
                    {
                        StockMonitorLogger.WriteMsg("ERROR, could not set instrument prices for stock with id " + (int)i.InsId.Value);
                    }
                }
            }
        }
    }
}
