﻿using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.Diagnostics.Metrics;


namespace Borsdata.Api.Dal.Model
{
#if !UNITTESTS
    internal class BD_API
    {
        HttpClient _client;                 // Important to NOT create new obj for each call. (Read docs about HttpClient) 
        string _querystring;                // Query string authKey
        Stopwatch _timer;                   // Check time from last API call to check rate limit
        string _urlRoot;

        public BD_API() {
            var lines = File.ReadAllLines("..\\..\\..\\..\\..\\BDKey.txt");

            if (lines.Any())
            {
                var key = lines[0];
                _client = new HttpClient();
                _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                _querystring = "?authKey=" + key;
                _timer = Stopwatch.StartNew();
                _urlRoot = "https://apiservice.borsdata.se";
                //_urlRoot = "https://bd-apimanager-dev.azure-api.net";
            }
        }

        /// <summary> Return end day stock price for one instrument</summary>
        public StockPricesRespV1 GetStockPrices(long instrumentId)
        {

            string url = $"{_urlRoot}/v1/instruments/{instrumentId}/stockprices";
            HttpResponseMessage response = WebbCall(url, _querystring);

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                StockPricesRespV1 res = JsonConvert.DeserializeObject<StockPricesRespV1>(json);
                return res;
            }
            else
            {
                Console.WriteLine("GetStockPrices {0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return null;
        }

        /// <summary> Return end day stock price for one instrument</summary>
        public StockPricesRespV1 GetStockPrices(long instrumentId, DateTime from, DateTime to)
        {
            string url = $"{_urlRoot}/v1/instruments/{instrumentId}/stockprices";
            string query = $"{_querystring}&from={from.ToShortDateString()}&to={to.ToShortDateString()}";

            HttpResponseMessage response = WebbCall(url, query);

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                StockPricesRespV1 res = JsonConvert.DeserializeObject<StockPricesRespV1>(json);
                return res;
            }
            else
            {
                Console.WriteLine("GetStockPrices time {0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return null;
        }

        /// <summary>
        /// Get stock prices for one instrument
        /// 20 year history
        /// </summary>
        static void StockPricesForOneInstruments(long InsId, string _apiKey)
        {
            BD_API api = new BD_API();
            StockPricesRespV1 spList = api.GetStockPrices(InsId);

            foreach (var sp in spList.StockPricesList)
            {
                Console.WriteLine(sp.D + " : " + sp.C);
            }
        }

        /// <summary> Return list of all reports for one instrument</summary>
        public ReportsRespV1 GetReports(long instrumentId, int maxYearCount = 20, int maxR12QCount = 20, int original = 0)
        {

            string url = $"{_urlRoot}/v1/instruments/{instrumentId}/reports";
            string query = $"{_querystring}&maxYearCount={maxYearCount}&maxR12QCount={maxR12QCount}&original={original}";

            HttpResponseMessage response = WebbCall(url, query);

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                ReportsRespV1 res = JsonConvert.DeserializeObject<ReportsRespV1>(json);
                return res;
            }
            else
            {
                Console.WriteLine("GetReports {0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return null;
        }


        /// <summary>
        /// Screener KPIs. Return one data point for one instrument.
        /// You can find exact API URL on Borsdata screener in the KPI window and [API URL] button.
        /// </summary>
        /// <param name="instrumentId">Company Ericsson has instrumentId=77</param>
        /// <param name="KpiId">KPI ID</param>
        /// <param name="time">Time period for the KPI</param>
        /// <param name="calc">Calculation format.</param>
        /// <returns></returns>
        public KpisRespV1 GetKpiScreenerSingle(long instrumentId, int KpiId, string time, string calc)
        {
            string url = $"{_urlRoot}/v1/Instruments/{instrumentId}/kpis/{KpiId}/{time}/{calc}";
            HttpResponseMessage response = WebbCall(url, _querystring);

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                KpisRespV1 res = JsonConvert.DeserializeObject<KpisRespV1>(json);
                return res;
            }
            else
            {
                Console.WriteLine("GetKpiScreenerSingle time {0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return null;
        }



        /// <summary>
        /// Screener KPIs. Return List of datapoints for all instruments.
        /// You can find exact API URL on Borsdata screener in the KPI window and [API URL] button.
        /// </summary>
        /// <param name="KpiId">KPI ID</param>
        /// <param name="time">Time period for the KPI</param>
        /// <param name="calc">Calculation format</param>
        /// <returns></returns>
        public KpisAllCompRespV1 GetKpiScreener(int KpiId, string time, string calc)
        {
            string url = $"{_urlRoot}/v1/Instruments/kpis/{KpiId}/{time}/{calc}";
            HttpResponseMessage response = WebbCall(url, _querystring);

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                KpisAllCompRespV1 res = JsonConvert.DeserializeObject<KpisAllCompRespV1>(json);
                return res;
            }
            else
            {
                Console.WriteLine("GetKpiScreener time {0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return null;
        }

        public MarketsRespV1 GetMarkets()
        {
            string url = $"{_urlRoot}/v1/markets";
            HttpResponseMessage response = WebbCall(url, _querystring);

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                MarketsRespV1 res = JsonConvert.DeserializeObject<MarketsRespV1>(json);
                return res;
            }
            else
            {
                Console.WriteLine("GetMarkets {0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return null;
        }
        public CountriesRespV1 GetCountries()
        {
            string url = $"{_urlRoot}/v1/countries";
            HttpResponseMessage response = WebbCall(url, _querystring);

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                CountriesRespV1 res = JsonConvert.DeserializeObject<CountriesRespV1>(json);
                return res;
            }
            else
            {
                Console.WriteLine("GetCountries {0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return null;
        }



        /// <summary> Return list of all instruments</summary>
        public InstrumentRespV1 GetInstruments()
        {
            string url = $"{_urlRoot}/v1/instruments";
            HttpResponseMessage response = WebbCall(url, _querystring);

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                InstrumentRespV1 res = JsonConvert.DeserializeObject<InstrumentRespV1>(json);
                return res;
            }
            else
            {
                Console.WriteLine("GetInstruments {0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return null;
        }



        /// <summary>
        /// Combine URL and query string. Check if 429 (rate limit).
        /// It tries to call API 2 times if rate limit is hit. This logic is ugly and only to demonstrate. 
        /// You schould use Polly or similar.
        /// </summary>
        /// <param name="url">API url</param>
        /// <param name="querystring">Querystring</param>
        /// <returns></returns>
        HttpResponseMessage WebbCall(string url, string querystring)
        {
            var w1 = Stopwatch.StartNew();
            HttpResponseMessage response = _client.GetAsync(url + querystring).Result; // Call API
            Console.WriteLine(url + querystring);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new Exception("Not authorized");

            if (response.StatusCode == HttpStatusCode.TooManyRequests) // We get RateLimit error. Sleep.
            {
                int sleepMs = (int)response.Headers.RetryAfter.Delta.Value.TotalMilliseconds; // Check header for time to wait (ms).
                Console.WriteLine($"Ratelimit sleep ms:{sleepMs}");
                System.Threading.Thread.Sleep(sleepMs);
                response = _client.GetAsync(url + querystring).Result; // Call API again

                if ((int)response.StatusCode == 429)
                    throw new Exception("Ratelimit hit twice!");
            }

            w1.Stop();
            //Console.WriteLine($"WebbCall : {url + querystring} ms: {w1.ElapsedMilliseconds}");
            return response;

        }
    }
}

#else
    // UNIT TEST SIMULATIONS
    internal class BD_API
    {
        HttpClient _client;                 // Important to NOT create new obj for each call. (Read docs about HttpClient) 
        string _querystring;                // Query string authKey
        Stopwatch _timer;                   // Check time from last API call to check rate limit
        string _urlRoot;

        List<StockPricesRespV1> _stockPricesRespV1 = new List<StockPricesRespV1>
        {
            new StockPricesRespV1
            {
                Instrument = 1,
                StockPricesList = new List<StockPriceV1> {
                new StockPriceV1 {
                    C = 100,
                    D = "2024-09-03"
                },
                new StockPriceV1 {
                    C = 90,
                    D = "2024-10-02"
                }
            }
            },
            new StockPricesRespV1
            {
                Instrument = 2,
                StockPricesList = new List<StockPriceV1> {
                new StockPriceV1 {
                    C = 80,
                    D = "2024-09-03"
                },
                new StockPriceV1 {
                    C = 120,
                    D = "2024-10-02"
                }
            }
        }};

        InstrumentRespV1 instrumentRespV1 = new InstrumentRespV1
        {
            Instruments = new List<InstrumentV1>
            {
                new InstrumentV1
                {
                    MarketId = 1,
                    InsId = 1,
                    Instrument = Borsdata.Api.Dal.Infrastructure.Instrument.Stocks,
                    Name = "SAAB"
                },
                new InstrumentV1
                {
                    MarketId = 2,
                    InsId = 2,
                    Instrument = Borsdata.Api.Dal.Infrastructure.Instrument.Stocks,
                    Name = "ABB"
                },
            }
        };

        MarketsRespV1 marketsRespV1 = new MarketsRespV1
        {
            Markets = new List<MarketV1>
            {
                new MarketV1
                {
                    Id = 1,
                    Name = "Large Cap"
                },
                new MarketV1
                {
                    Id = 2,
                    Name = "Mid Cap"
                },
                new MarketV1
                {
                    Id = 2,
                    Name = "Small Cap"
                },
            },
        };

        public void setKey(string key)
        {

        }

        

        /// <summary> Return end day stock price for one instrument</summary>
        public StockPricesRespV1 GetStockPrices(long instrumentId, DateTime from, DateTime to)
        {
            // TODO; skickar allt just nu, 
            return _stockPricesRespV1.Find(p => (long)p.Instrument == instrumentId);
        }

        public MarketsRespV1 GetMarkets()
        {
            return marketsRespV1;
        }



        /// <summary> Return list of all instruments</summary>
        public InstrumentRespV1 GetInstruments()
        {
            return instrumentRespV1;
        }
    }
}



#endif
