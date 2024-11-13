using StocksMonitor.src.databaseWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksMonitor.src.Simulation
{
    public struct Configuration {
        public bool dividentRequired;
        public bool profitRequired;
        public bool indexCalculation;

        public bool balanceInvestment;


        public List<Rule> buyRules;
        public List<Rule> sellRules;
        public List<Rule> adjustBuyRules;
    };

    public class SimulationConfiguration
    {
        public static readonly Dictionary<TMarket, string> markets = new Dictionary<TMarket, string>
        {
            { TMarket.All, "All Sthlm" },
            { TMarket.AllExceptFirstNorth, "All except First North" },
            { TMarket.LargeCap, "Large Cap" },
            { TMarket.MidCap, "Mid Cap" },
            { TMarket.SmallCap, "Small Cap" },
            { TMarket.FirstNorth, "First North" },
            { TMarket.IndexFirstNorthAll, "First North All" },
            { TMarket.IndexOMXSmallCap, "OMX Small Cap" },
            { TMarket.IndexOMXMidCap, "OMX Mid Cap" },
            { TMarket.IndexOMXLargeCap, "OMX Large Cap" },
            { TMarket.IndexOMXSGI, "OMX Stockholm GI" }
        };

        public Configuration configuration;
        public decimal Investment { get; private set; } = 0;
        public decimal Value { get; private set; } = 0;
        public decimal Wallet { get; private set; } = 0;
        public TMarket stockMarket;


        List<Stock> simulatorStocks = [];  // TODO, make readonly
        public string name { get; set; }

        public decimal oneMonth { get; private set; } = 0;
        public decimal sixMonths { get; private set; } = 0;
        public decimal oneYear { get; private set; } = 0;
        public decimal twoYears { get; private set; } = 0;
        public decimal fiveYears { get; private set; } = 0;
        public decimal tenYears { get; private set; } = 0;
        public decimal fifteenYears { get; private set; } = 0;


        public SimulationConfiguration()
        {
            configuration.buyRules = [];
            configuration.sellRules = [];
            configuration.adjustBuyRules = [];
        }

        public string getNameString()
        {
            string name = markets[stockMarket];

            if (configuration.indexCalculation)
            {
                name += " index";
            }
            else
            {

                name += ": Buy: ";
                foreach (var rule in configuration.buyRules)
                {
                    name += rule.rule + " " + rule.RuleValue;
                }
                name += ": adjust: ";
                foreach (var rule in configuration.adjustBuyRules)
                {
                    name += rule.rule + " " + rule.RuleValue;
                }

                name += ". Sell: ";
                foreach (var rule in configuration.sellRules)
                {
                    name += rule.rule + " " + rule.RuleValue;
                }
            }

            if (configuration.balanceInvestment)
            {
                name += " balance inv.";
            }

            if (configuration.dividentRequired)
            {
                name += ". Div";
            }
            if (configuration.profitRequired)
            {
                name += ". Prof";
            }

            return name;
        }




        public void Init(List<Stock> stocks)
        {
            IEnumerable<Stock> filtred = stocks;
            configuration.indexCalculation = false;
            // TODO, add warnings if filters stop working?
            switch (stockMarket)
            {
                case TMarket.IndexFirstNorthAll:
                    filtred = stocks.Where(s => s.Name == "First North All");
                    configuration.indexCalculation = true;
                    break;
                case TMarket.IndexOMXSmallCap:
                    filtred = stocks.Where(s => s.Name == "OMX Small Cap");
                    configuration.indexCalculation = true;
                    break;
                case TMarket.IndexOMXMidCap:
                    filtred = stocks.Where(s => s.Name == "OMX Mid Cap");
                    configuration.indexCalculation = true;
                    break;
                case TMarket.IndexOMXLargeCap:
                    filtred = stocks.Where(s => s.Name == "OMX Large Cap");
                    configuration.indexCalculation = true;
                    break;
                case TMarket.IndexOMXSGI:
                    filtred = stocks.Where(s => s.Name == "OMX Stockholm GI");
                    configuration.indexCalculation = true;
                    break;

                case TMarket.AllExceptFirstNorth:
                    filtred = stocks.Where(s => s.List != markets[TMarket.FirstNorth] && s.List != "Index");
                    break;

                case TMarket.LargeCap:
                case TMarket.MidCap:
                case TMarket.SmallCap:
                case TMarket.FirstNorth:
                    filtred = stocks.Where(s => s.List == markets[stockMarket]);
                    break;

                case TMarket.All:
                default:
                    filtred = stocks.Where(s => s.List != "Index");
                    break;
            }

            if (configuration.dividentRequired)
            {
                filtred = filtred.Where(s => s.Divident > 0);
            }
            if (configuration.profitRequired)
            {
                filtred = filtred.Where(s => s.PeValue > 0);
            }
            simulatorStocks = filtred.ToList();

        }

        // TODO, add PE And divident to stockData, no need to save to history, just latest data is enough
        // TODO, kanske ska simuleringarna att köra, men att jag kan ställa saker i någon "konfig" investeringssumma, adj rate, marknadsval, ma justeringar / nivåer

        // TODO, flytta strategier till simulering, så jag kan hantera smidigare i helhet. 
        // t.,exa att titta på historik, för att hitta MA brytpunkter, eller att sortera datan, på .tex. närmast MA för att köpa först där, isf det som ligger högst upp på intervall.


        public List<Stock> getStocksFromDate(DateTime from)
        {
            return simulatorStocks
                .Select(stock => new Stock
                {
                    Name = stock.Name,
                    MA200 = stock.MA200,
                    Divident = stock.Divident,
                    List = stock.List,
                    OwnedCnt = 0,
                    PeValue = stock.PeValue,
                    PurPrice = stock.PurPrice,
                    History = stock.History
                    .Where(h => h.Date >= from)
                    .OrderBy(h => h.Date)
                    .ToList()
                })
            .ToList();
        }

        public void Run(List<Stock> stocks)
        {
            // TODO skriva owned cnt i stocklistan, per historik, för att få koll på utvekcling, väldigt lik vanliga procentuella uträkningen också
            Init(stocks); // filter out stocks for interesting market 

            var stockHistories = simulatorStocks.SelectMany(s => s.History).OrderBy(h => h.Date);

            if (stockHistories.Last() == null)
            {
                return;
            }
            var newestDate = stockHistories.Last().Date;

            var oneMonthSim = new SimulationNew(
                getStocksFromDate(newestDate.AddMonths(-1)),
                configuration: configuration);
            var sixMonthSim = new SimulationNew(
                getStocksFromDate(newestDate.AddMonths(-1)),
                configuration: configuration);
            var oneYearSim = new SimulationNew(
                getStocksFromDate(newestDate.AddYears(-1)),
                configuration: configuration);
            var twoYearsSim = new SimulationNew(
                getStocksFromDate(newestDate.AddYears(-2)),
                configuration: configuration);
            var fiveYearsSim = new SimulationNew(
                getStocksFromDate(newestDate.AddYears(-5)),
                configuration: configuration);
            var tenYearsSim = new SimulationNew(
                getStocksFromDate(newestDate.AddYears(-10)),
                configuration: configuration);
            var fifteenYearsSim = new SimulationNew(
                getStocksFromDate(newestDate.AddYears(-15)),
                configuration: configuration);

            List<SimulationNew> simulations = [
                oneMonthSim,
                sixMonthSim,
                oneYearSim,
                twoYearsSim,
                fiveYearsSim,
                tenYearsSim,
                fifteenYearsSim,
            ];

            
            var tasks = new List<Task>();

            foreach(var sim in simulations)
            {
                tasks.Add(
                    Task.Run(() => sim.SimulateStocks()
                    ));
            }
            Task.WhenAll(tasks).Wait();

            oneMonth = oneMonthSim.result;
            sixMonths = sixMonthSim.result;
            oneYear = oneYearSim.result;
            twoYears = twoYearsSim.result;  
            fiveYears = fiveYearsSim.result;
            tenYears = tenYearsSim.result;
            fifteenYears = fifteenYearsSim.result;  
        }
    }
}
