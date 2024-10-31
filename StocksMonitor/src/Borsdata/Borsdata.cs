using Borsdata.Api.Dal.Infrastructure;
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
        private Dictionary<string, long> _CountriesId;

        const string largeCap = "Large Cap";
        const string midCap = "Mid Cap";
        const string smallCap = "Small Cap";
        const string firstNorth = "First North";

        const string countrySE = "Sverige";


         public readonly List<string> indexes = new List<String> {
            "First North All",
            "OMX Small Cap",
            "OMX Mid Cap",
            "OMX Large Cap",
            "OMX Stockholm GI"
        };

        public Dictionary<string, List<StockPriceV1>> InstrumentPrices { get; set; }

        public BorsData() 
        {
            InstrumentPrices = new Dictionary<string, List<StockPriceV1>>();
            _marketsId = new Dictionary<string, long>();
            _CountriesId = new Dictionary<string, long>();
            _api = new BD_API();


        }

        public string GetMarketName(string instrumentName)
        {

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

            var newestValue = values.First().C;
            // to take the latest 200, need to reverse list, since data is from old --> new. newest is last
            var sum = values.Take(selectedMa).Sum(s => s.C);

            var ma200 = sum / selectedMa;
            var ma200Percent = (decimal)((newestValue - ma200) / ma200 * 100);

            return ma200Percent;
        }


        public void Run()
        {
            GetAllCountries();
            GetAllMarkets();
            GetAllInstruments();
            FillStockPrices();

        }
        public void GetAllMarkets()
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

        public void GetAllCountries()
        {
            StockMonitorLogger.WriteMsg("Get all countries from BD");

            var _countries = _api.GetCountries();
            if (_countries != null)
            {
                foreach (var country in _countries.Countries)
                {
                    if (country.Id.HasValue)
                    {
                        if (!_CountriesId.ContainsKey(country.Name))
                        {
                            _CountriesId.Add(country.Name, country.Id.GetValueOrDefault());
                        }
                    }
                    else
                    {
                        StockMonitorLogger.WriteMsg("WARNING, could not read market id of " + country.Name);
                    }
                }
            }
        }

        public void GetAllInstruments()
        {
            StockMonitorLogger.WriteMsg("Get all instruments from BD");
            _instruments = _api.GetInstruments().Instruments;

        }

        private void FillStockPrices()
        {
            StockMonitorLogger.WriteMsg("Fill stock prices from BD");

            // Only SE List
            _instruments.RemoveAll(i => i.CountryId != _CountriesId[countrySE]);

            GatherDataFromInstruments(_instruments.Where(i => i.MarketId ==
                      _marketsId[largeCap]).ToList());
            GatherDataFromInstruments(_instruments.Where(i => i.MarketId ==
                      _marketsId[midCap]).ToList());
            GatherDataFromInstruments(_instruments.Where(i => i.MarketId ==
                      _marketsId[smallCap]).ToList());
            GatherDataFromInstruments(_instruments.Where(i => i.MarketId ==
                      _marketsId[firstNorth]).ToList());
     
            foreach(var index in indexes)
            {
                GatherDataFromInstruments(
                    _instruments.Where(i => i.Name == index).ToList()
                );
            }
        }

        private void GatherDataFromInstruments(List<InstrumentV1> instruments)
        {
            // Get all stock prices for each instrument
            foreach (var instrument in instruments)
            {
                //--Get for a time range
                // TODO, läser just nu data för senaste året

                // TODO, kör första är äldstsa datum när jag läst in till histoiken
                StockPricesRespV1 sp = _api.GetStockPrices(instrument.InsId.Value, DateTime.Today.AddYears(-2), DateTime.Today);

                //StockPricesRespV1 sp = _api.GetStockPrices(i.InsId.Value);
                if (sp != null)
                {
                    var matchedInstrument = _instruments.Find(i => i.InsId == instrument.InsId);

                    if (matchedInstrument != null)
                    {
                        var instrumentName = matchedInstrument.Name;
                        InstrumentPrices[instrumentName] = sp.StockPricesList;
                    }
                    else
                    {
                        StockMonitorLogger.WriteMsg("ERROR: Could not set instrument prices for stock with ID " + instrument.InsId);
                    }
                }
            }
        }
    }
}
