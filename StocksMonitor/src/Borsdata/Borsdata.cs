﻿using Borsdata.Api.Dal.Infrastructure;
using Borsdata.Api.Dal.Model;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StocksMonitor.src.Borsdata
{
    public class InstrumentData
    {
        public decimal PE;
        public decimal Divident;
        public List<StockPriceV1> prices;
    }
    public class BorsData
    {
        private List<InstrumentV1> _instruments;
        private BD_API _api;
        private Dictionary<string, long> _marketsId;
        private Dictionary<string, long> _CountriesId;


        //<markets
        const string marketLargeCap = "Large Cap";
        const string marketMidcap = "Mid Cap";
        const string marketSmallCap = "Small Cap";
        const string marketFirstNorth = "First North";
        const string marketIndex = "Index";

        const string countrySE = "Sverige";


         public readonly List<string> indexes = new List<String> {
            "First North All",
            "OMX Small Cap",
            "OMX Mid Cap",
            "OMX Large Cap",
            "OMX Stockholm GI"
        };

        public Dictionary<string, InstrumentData> InstrumentDatas { get; set; }

        public BorsData() 
        {
            InstrumentDatas = new Dictionary<string, InstrumentData>();
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
            GetKpiScreener();

            FilterReadData();    // TODO, maybe do this in simulation, to make it more clear the effect of it
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

        public void GetKpiScreener()
        {
            StockMonitorLogger.WriteMsg("Get KPI Screener from BD");
            const int dividentKey = 1;
            const int peKey = 2;


            foreach (var instrument in _instruments)
            {

                var PE = _api.GetKpiScreenerSingle((long)instrument.InsId, peKey, TimeType.last, CalcType.latest);
                var div = _api.GetKpiScreenerSingle((long)instrument.InsId, dividentKey, TimeType.last, CalcType.latest);

                try
                {
                    InstrumentDatas[instrument.Name].Divident = (decimal)(div?.Value.N ?? 0);
                    InstrumentDatas[instrument.Name].PE = (decimal)(PE?.Value.N ?? 0);
                }
                catch (Exception ex)
                {
                    StockMonitorLogger.WriteMsg("WARNING, could not enter instrument data with name " + instrument.Name);        
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

            foreach (var index in indexes)
            {
                GatherDataFromInstruments(
                    _instruments.Where(i => i.Name == index).ToList()
                );
            }

            //TODO refactor

            // Only SE List
            _instruments.RemoveAll(i => i.CountryId != _CountriesId[countrySE]);

            var wantedMarketIds = _marketsId.Where( i => 
                i.Key == marketLargeCap || 
                i.Key == marketMidcap || 
                i.Key == marketSmallCap || 
                i.Key == marketFirstNorth ||
                i.Key == marketIndex
                ).Select(m => m.Value).ToList();

            // remove all that does not belong to the markeids define
            _instruments.RemoveAll(i => !wantedMarketIds.Contains((long)i.MarketId));

            GatherDataFromInstruments(_instruments);
        }

        private void FilterReadData()
        {
            // REMOVE Stocks, that is to low value, --> Price < 5KR

            // last element is newest
            InstrumentDatas = InstrumentDatas
            .Where(i => (decimal)i.Value.prices.LastOrDefault().C >= 5m)
            .ToDictionary(i => i.Key, i => i.Value);
        }

        private void GatherDataFromInstruments(List<InstrumentV1> instruments)
        {
            // Get all stock prices for each instrument
            foreach (var instrument in instruments)
            {
                //--Get for a time range
                // TODO, läser just nu data för senaste året

                // TODO, kör första är äldstsa datum när jag läst in till histoiken
                StockPricesRespV1 sp = _api.GetStockPrices(instrument.InsId.Value, DateTime.Today.AddYears(-19), DateTime.Today);

                //StockPricesRespV1 sp = _api.GetStockPrices(i.InsId.Value);
                if (sp != null)
                {
                    var matchedInstrument = _instruments.Find(i => i.InsId == instrument.InsId);

                    if (matchedInstrument != null)
                    {
                        var instrumentName = matchedInstrument.Name;
                        InstrumentDatas[instrumentName] = new InstrumentData()
                        {
                            prices = sp.StockPricesList,
                        };
                            
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
