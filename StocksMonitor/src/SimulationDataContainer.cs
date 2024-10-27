using GrapeCity.DataVisualization.Chart;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using StocksMonitor.src;
using StocksMonitor.src.databaseWrapper;
using StocksMonitor.src.dataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;


#if SIMULATIONS
namespace StocksMonitor.src
{
    public class Simulation {

        public static readonly Dictionary<TMarket, string> markets = new Dictionary<TMarket, string>   
        {
        { TMarket.All, "All Sthlm" },
        { TMarket.AllExceptFirstNorth, "All Sthlm exc. First North" },
        { TMarket.LargeCap, "Large Cap Stockholm" },
        { TMarket.MidCap, "Mid Cap Stockholm" },
        { TMarket.SmallCap, "Small Cap Stockholm" },
        { TMarket.FirstNorth, "First North Stockholm" }
        };
    

        public List<Rule> buyRules;
        public List<Rule> sellRules;
        public TMarket stockMarket;
        public bool dividentRequired;
        public bool profitRequired;
        public bool indexCalculation;

        readonly decimal originalInvestment = 100000;
        const decimal investmentTarget = 500;

        List<Stock> simulatorStocks = [];
        public string name { get; set; }
        public decimal oneDay { get; private set; } = 0;
        public decimal oneWeek { get; private set; } = 0;
        public decimal oneMonth { get; private set; } = 0;
        public decimal oneYear { get; private set; } = 0;
        public decimal Investment { get; private set; } = 0;
        public decimal Value { get; private set; } = 0;
        public decimal Wallet { get; private set; } = 0;
        public decimal TotDevelopment { get; private set; } = 0;

        public Simulation()
        {
            buyRules = [];
            sellRules = [];
        }

        public string getNameString()
        {
            string name = markets[stockMarket];

            if(indexCalculation)
            {
                name += " index";
            }
            else {
                name += ": Buy";
                var rule = buyRules.Find(r => r.rule == TRule.AboveMa);
                if (rule != null)
                {
                    name += ": MA > " + rule.value;
                }
                buyRules.Find(r => r.rule == TRule.BelowMa);
                if (rule != null)
                {
                    name += ": & < " + rule.value;
                }

                sellRules.Find(r => r.rule == TRule.Never);
                if (rule != null)
                {
                    name += " and keep";
                }
            }

            if(dividentRequired)
            {
                name += ". Div";
            }
            if (profitRequired)
            {
                name += ". Prof";
            }

            return name;
        }


        private void AddToWallet(decimal value)
        {
            Wallet += value;
            Investment += value;
        }

        
        public void Init(List<Stock> stocks, bool indexCalculation = false)
        {
            IEnumerable<Stock> filtred = stocks;

            switch (stockMarket)
            {
                case TMarket.AllExceptFirstNorth:
                    filtred = stocks.Where(s => s.List != markets[TMarket.FirstNorth]);
                    break;

                case TMarket.LargeCap:
                case TMarket.MidCap:
                case TMarket.SmallCap:
                case TMarket.FirstNorth:
                    filtred = stocks.Where(s => s.List != markets[stockMarket]);
                    break;

                case TMarket.All:
                default:
                    filtred = stocks;
                    break;
            }

            if (dividentRequired)
            {
                filtred = filtred.Where(s => s.Divident > 0);
            }
            if (profitRequired)
            {
                filtred = filtred.Where(s => s.PeValue > 0);
            }
            simulatorStocks = filtred.ToList();

            if (!indexCalculation)
            {
                AddToWallet(originalInvestment);
            }

        }

        public Strategy strategy;
        public List<Portfolio> portfolioHistory = [];

        private void ClearOldData()
        {
            foreach (var stock in simulatorStocks)
            {
                stock.OwnedCnt = 0;
                foreach (var history in stock.History)
                {
                    history.OwnedCnt = 0;
                }
            }
        }


        // TODO, add PE And divident to stockData, no need to save to history, just latest data is enough
        // TODO, kanske ska simuleringarna att köra, men att jag kan ställa saker i någon "konfig" investeringssumma, adj rate, marknadsval, ma justeringar / nivåer
        private decimal CalculateDevelopmentBetweenDates(DateTime oldDate, DateTime currentDate)
        {
            var currentPortfolio = portfolioHistory.Find(p => p.timestamp == currentDate.Date);
            var oldPortfolio = portfolioHistory.Find(p => p.timestamp == oldDate.Date);

            if (currentPortfolio == null || oldPortfolio == null)
            {
                StockMonitorLogger.WriteMsg("ERROR, could not find dates to calculate portfolio development, Skipping");
                return 0;
            }
            var currentVal = currentPortfolio.value + currentPortfolio.wallet;
            var oldVal = oldPortfolio.value + oldPortfolio.wallet;

            var diff = currentVal - oldVal;
            return diff / oldVal * 100;
        }
        // TODO, flytta strategier till simulering, så jag kan hantera smidigare i helhet. 
        // t.,exa att titta på historik, för att hitta MA brytpunkter, eller att sortera datan, på .tex. närmast MA för att köpa först där, isf det som ligger högst upp på intervall.
        public void CalculateSimulationResult()
        {
            portfolioHistory.Reverse();

            if (!portfolioHistory.Any())
            {
                StockMonitorLogger.WriteMsg("ERROR, Could not find first and last entry from history, ABORT");
                return;
            }

            var currentDay = portfolioHistory.First();

            TotDevelopment =
                CalculateDevelopmentBetweenDates(portfolioHistory.Last().timestamp, portfolioHistory.First().timestamp);

            Value = currentDay.value;

            foreach (var history in portfolioHistory)
            {
                var timespan = currentDay.timestamp - history.timestamp;

                if (oneDay == 0 && timespan.TotalDays >= 1)
                {
                    oneDay = CalculateDevelopmentBetweenDates(history.timestamp, currentDay.timestamp);
                }
                else if (oneWeek == 0 && timespan.TotalDays >= 7)
                {
                    oneWeek = CalculateDevelopmentBetweenDates(history.timestamp, currentDay.timestamp);
                }
                else if (oneMonth == 0 && timespan.TotalDays > 31)
                {
                    oneMonth = CalculateDevelopmentBetweenDates(history.timestamp, currentDay.timestamp);
                }
                else if (oneYear == 0 && timespan.TotalDays >= 365)
                {
                    oneYear = CalculateDevelopmentBetweenDates(history.timestamp, currentDay.timestamp);
                }
            }
        }

        protected (int, decimal) CalculateCost(History dataPoint, decimal wallet)
        {
            int buyCount = (int)((investmentTarget - (dataPoint.OwnedCnt * dataPoint.Price)) / dataPoint.Price);
            decimal cost = (dataPoint.Price * buyCount);

            var walletAllows = cost > 0 && wallet >= cost;

            if (buyRules.Any(r => r.rule == TRule.Index))
            {
                // no wallet simulation
                walletAllows = true;
                cost = 0;
                buyCount = 1;
            }
            

            return walletAllows ? (buyCount, cost) : (0, 0);
        }

        private bool ComplyToRules(List<Rule> rules, History datapoint)
        {
            if(rules.Any(r => r.rule == TRule.Never))
            {
                return false;
            }

            foreach (var rule in rules)
            {
                switch (rule.rule)
                {
                    case TRule.AboveMa:
                        if (datapoint.Price < (datapoint.MA200 + Value))
                        {
                            return false;
                        }
                        break;
                    case TRule.BelowMa:
                        if (datapoint.Price > (datapoint.MA200 + Value))
                        {
                            return false;
                        }
                        break;

                }
            }
            return true;
                    
        }
        private void HandleDatapoint(Stock stock, History datapoint, decimal wallet)
        {
            // copy latest status, to have as base for decision
            datapoint.OwnedCnt = stock.OwnedCnt;
            //h.Price = stock.Price;

            if (datapoint.OwnedCnt == 0) {
                if(ComplyToRules(buyRules, datapoint))
                {
                    var (count, price) = CalculateCost(datapoint, Wallet);
                    Wallet -= price;
                    datapoint.OwnedCnt += count;
                }
            }
            else
            {

            }

/*
            
            // calculate actions
            var datapointResult = strategy.DetermineAction(dataPoint: datapoint, wallet: Wallet);
            switch (datapointResult.action)
            {
                case StratAction.BUY:
                    Wallet -= datapointResult.value;
                    datapoint.OwnedCnt += datapointResult.adjustmentCount;
                    break;
                case StratAction.SELL:
                // fall-through
                case StratAction.ADJ_DOWN:
                    Wallet += datapointResult.value;
                    datapoint.OwnedCnt -= datapointResult.adjustmentCount;
                    break;
                case StratAction.NONE:
                    break;
            }
*/
            // update stock status, from possible actions
            stock.OwnedCnt = datapoint.OwnedCnt;
            //stock.Price = h.Price;
        }

        public void Run()
        {
            // TODO skriva owned cnt i stocklistan, per historik, för att få koll på utvekcling, väldigt lik vanliga procentuella uträkningen också

            // latest hisory is the same as current, dont duplicate
            var oldestTimeStamp = simulatorStocks.SelectMany(s => s.History).OrderBy(h => h.Date).FirstOrDefault();
            var newestTimeStamp = simulatorStocks.SelectMany(s => s.History).OrderBy(h => h.Date).LastOrDefault();
            ClearOldData();

            if (oldestTimeStamp == null || newestTimeStamp == null)
            {
                StockMonitorLogger.WriteMsg("ABORT, Could not find timestamps, ABORT");
                return;
            }
            // History när läst från db går från äldsta i 0 --> 27/9, till nyaste sist 14 --> 22/10
            var simulationDay = oldestTimeStamp.Date;

            while (simulationDay != newestTimeStamp.Date.AddDays(1).Date)
            {
                foreach (var stock in simulatorStocks)
                {
                    var h = stock.History.FirstOrDefault(h => h.Date == simulationDay);
                    // TODO, datagrid och graf, kan yllas anting med simulator data eller utevklingsdata. Utvecklingsdata kan komma från simulering eller inte.

                    if (h != null)
                    {
                        HandleDatapoint(stock, h, Wallet);
                    }
                }
                var allStocksHistoryDay = simulatorStocks.SelectMany(s => s.History).Where(h => h.Date == simulationDay);

                if (allStocksHistoryDay.Any())
                {
                    portfolioHistory.Add(
                        new Portfolio(
                            date: simulationDay,
                            wallet: Wallet,
                            value: allStocksHistoryDay.Sum(h => h.OwnedCnt * h.Price),
                            investment: Investment
                        ));
                }
                simulationDay = simulationDay.AddDays(+1);
            }
            // TODOD, grafen när det gäller simuleringarna, visar bara portfölhistory, i procentutveckling steg för steg
            CalculateSimulationResult();
        }

    }


    public class DataContainer
    {
        private DataGridView dataGrid;
        private DataStore store;
        private int investmentTarget = 500; // TODO, make adjustable

        private List<Simulation> simulations;

        public DataContainer(DataGridView dataGridView, DataStore store)
        {
            this.dataGrid = dataGridView;
            this.store = store;
            this.simulations = new ();
        }

        public void init()
        {
            dataGrid.Columns.Clear(); // Clear any existing columns

            // Define your columns
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Name",
                ValueType = typeof(String)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "d %",
                HeaderText = "d %",
                ToolTipText = "1 day",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "w %",
                HeaderText = "w %",
                ToolTipText = "1 week",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "m %",
                HeaderText = "m %",
                ToolTipText = "1 month",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "y %",
                HeaderText = " y %",
                ToolTipText = "1 year",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Inv",
                HeaderText = "Inv",
                ToolTipText = "Investment",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Val",
                HeaderText = "Val",
                ToolTipText = "Value",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Wall",
                HeaderText = "Wall",
                ToolTipText = "Wallet",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Tot %",
                HeaderText = "Tot %",
                ToolTipText = "Total development",
                ValueType = typeof(decimal)
            });

            // Set properties for better appearance
            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                dataGrid.Columns[i].DefaultCellStyle.Format = "N2";  // Max two decimals
                if (i == 0)
                {
                    // name shall be leftaligned, rest, center
                    dataGrid.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    continue;
                }
                dataGrid.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            dataGrid.Columns[0].MinimumWidth = 250;
        }

        public void UpdateData(bool warnings, bool hidden, bool wanted, bool intrested, bool owned)
        {
        }
        private List<Simulation> generateSimulations()
        {
            List<Simulation> returnSims = new List<Simulation>();

            returnSims.AddRange(
            new List<Simulation>
            {
                    new Simulation()
                    {
                        name = "All Market, Buy 0 < MA > 15, and keep stock",
                        stockMarket = TMarket.All,
                        buyRules =
                        {
                            new Rule (TRule.AboveMa, 0),
                            new Rule (TRule.BelowMa, 15)
                        },
                        sellRules =
                        {
                            new Rule(TRule.Never)
                        }
                    }

            });

            foreach (var market in Simulation.markets)
            {
                /*
                 * div = true, profit false
                 * div = true, profit true
                 * div = false, profit false,
                 * div = false, profit true
                 */

                for(var i = 0; i < 4; i++)
                {
                    returnSims.Add(
                    new Simulation()
                    {
                        stockMarket = market.Key,
                        indexCalculation = true,
                        dividentRequired = i < 2,
                        profitRequired = i % 2 != 0,

                        buyRules =
                        {
                            new Rule (TRule.Index)
                        },
                        sellRules =
                        {
                            new Rule(TRule.Never)
                        }
                    });
                }
            }

            return returnSims;

            // TODO, kan generera namn, från TOString() eller så, för objekten, så de genereras. När datacolum skivs
        }

        
        public void UpdateData()
        {
            dataGrid.Rows.Clear(); // Clear existing rows

            simulations.AddRange(generateSimulations());
                
            foreach (var sim in simulations)
            {
                sim.Init(store.stocks);
                sim.Run();
                AddSimulationToDataGrid(sim);
            }
        }


        public void AddSimulationToDataGrid(Simulation sim)
        {
            DataGridViewRow row = new DataGridViewRow();
            
            //var stockDevelopement = sim.strategy.GetType() == typeof(StockDevelopmentSimulation);

            row.CreateCells(dataGrid,
                sim.getNameString(),
                sim.oneDay,
                sim.oneWeek,
                sim.oneMonth,
                sim.oneYear,
                sim.Investment,
                sim.Value,
                sim.Wallet,
                sim.TotDevelopment
                );

            dataGrid.Rows.Add(row);
        }
    }
    public class Portfolio
    {
        public decimal wallet;
        public decimal value;
        public decimal investment;
        public DateTime timestamp;


        public Portfolio(DateTime date, decimal wallet, decimal investment, decimal value)
        {
            this.timestamp = date;
            this.wallet = wallet;
            this.investment = investment;
            this.value = value;
        }
    }

  
}
#endif