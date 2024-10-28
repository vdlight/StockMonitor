using Microsoft.EntityFrameworkCore.Metadata;
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
    internal class BD_API
    {
        HttpClient _client;                 // Important to NOT create new obj for each call. (Read docs about HttpClient) 
        string _querystring;                // Query string authKey
        Stopwatch _timer;                   // Check time from last API call to check rate limit
        string _urlRoot;

        public BD_API(string apiKey) { 
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            _querystring = "?authKey=" + apiKey;
            _timer = Stopwatch.StartNew();
            _urlRoot = "https://apiservice.borsdata.se";
            //_urlRoot = "https://bd-apimanager-dev.azure-api.net";
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
            BD_API api = new BD_API(_apiKey);
            StockPricesRespV1 spList = api.GetStockPrices(InsId);

            foreach (var sp in spList.StockPricesList)
            {
                Console.WriteLine(sp.D + " : " + sp.C);
            }
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

