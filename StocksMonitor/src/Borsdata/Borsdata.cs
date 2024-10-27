using Borsdata.Api.Dal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksMonitor.src.Borsdata
{
    public class BorsData
    {
        public InstrumentRespV1 _instRespV1;
        ApiClient _api;
        private string _apiKey = "f2f9bceb531d44d2b71f20a3b17475f6";
        public Dictionary<long, List<StockPriceV1>> _instrumentPrices;
        public Dictionary<string, long> _marketsId;

        const string largeCap = "Large Cap";
        const string midCap = "Mid Cap";
        const string smallCap = "Small Cap";
        const string firstNorth = "First North";


        public BorsData() 
        {
            _instrumentPrices = new Dictionary<long, List<StockPriceV1>>();
            _marketsId = new Dictionary<string, long>();    
        }

        public void Run()
        {
            _api = new ApiClient(_apiKey);
            GetAllMarkets();

            GetAllInstruments();
            FillStockPrices();
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
            _instRespV1 = _api.GetInstruments();
        }

        private void FillStockPrices()
        {
            StockMonitorLogger.WriteMsg("Fill stock prices from BD");

            FillInstrumentsFromMarket(_instRespV1.Instruments.Where(i => i.MarketId == 
                _marketsId[largeCap]).ToList());
            FillInstrumentsFromMarket(_instRespV1.Instruments.Where(i => i.MarketId == 
                _marketsId[midCap]).ToList());
            FillInstrumentsFromMarket(_instRespV1.Instruments.Where(i => i.MarketId ==
                _marketsId[smallCap]).ToList());
            FillInstrumentsFromMarket(_instRespV1.Instruments.Where(i => i.MarketId ==
                _marketsId[firstNorth]).ToList());

        }

        private void FillInstrumentsFromMarket(List<InstrumentV1> instruments)
        {
            // Get all stock prices for each instrument
            foreach (var i in instruments)
            {
                //--Get for a time range
                // TODO, läser just nu data för senaste året

                // TODO, kör första är äldstsa datum när jag läst in till histoiken
                // MA 200 needs 201
                StockPricesRespV1 sp = _api.GetStockPrices(i.InsId.Value, DateTime.Today.AddDays(-400), DateTime.Today);

                //StockPricesRespV1 sp = _api.GetStockPrices(i.InsId.Value);
                if (sp != null)
                {
                    _instrumentPrices[i.InsId.Value] = sp.StockPricesList;
                }
            }
        }
    }
}
