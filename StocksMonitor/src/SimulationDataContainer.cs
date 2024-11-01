using GrapeCity.DataVisualization.Chart;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using StocksMonitor.src;
using StocksMonitor.src.databaseWrapper;
using StocksMonitor.src.dataStoreNS;
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
        public decimal oneWeek { get; private set; } = 0;
        public decimal oneMonth { get; private set; } = 0;
        public decimal sixMonths { get; private set; } = 0;
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

                name += ": Buy: ";
                foreach(var rule in buyRules)
                {
                    name += rule.rule + " " + rule.RuleValue; 
                }

                name += ". Sell: ";
                foreach (var rule in sellRules)
                {
                    name += rule.rule + " " + rule.RuleValue;
                }
            }

            if (dividentRequired)
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
            // TODO, add warnings if filters stop working?
            switch (stockMarket)
            {
                case TMarket.IndexFirstNorthAll:
                    filtred = stocks.Where(s => s.Name == "First North All");
                    break;
                case TMarket.IndexOMXSmallCap:
                        filtred = stocks.Where(s => s.Name == "OMX Small Cap");
                    break;
                case TMarket.IndexOMXMidCap:
                    filtred = stocks.Where(s => s.Name == "OMX Mid Cap");
                    break;
                case TMarket.IndexOMXLargeCap:
                    filtred = stocks.Where(s => s.Name == "OMX Large Cap");
                    break;
                case TMarket.IndexOMXSGI:
                    filtred = stocks.Where(s => s.Name == "OMX Stockholm GI");
                    break;

                case TMarket.AllExceptFirstNorth:
                    filtred = stocks.Where(s => s.List != markets[TMarket.FirstNorth]);
                    break;

                case TMarket.LargeCap:
                case TMarket.MidCap:
                case TMarket.SmallCap:
                case TMarket.FirstNorth:
                    filtred = stocks.Where(s => s.List == markets[stockMarket]);
                    break;

                case TMarket.All:
                default:
                    filtred = stocks.Where(s=> s.List != "Index");
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
            portfolioHistory.Clear();
        }


        // TODO, add PE And divident to stockData, no need to save to history, just latest data is enough
        // TODO, kanske ska simuleringarna att köra, men att jag kan ställa saker i någon "konfig" investeringssumma, adj rate, marknadsval, ma justeringar / nivåer
        private decimal CalculateDevelopmentBetweenDates(DateTime oldDate, DateTime currentDate)
        {
            var currentDay = currentDate;
            var oldDay = oldDate;

            var currentPortfolio = portfolioHistory.Find(p => p.timestamp == currentDate.Date);
            var searchLimit = 0;

            while(currentPortfolio == null && searchLimit < 15)
            {
                currentDate = currentDate.AddDays(-1); // if not find on current day, adjust to previous day, 
                currentPortfolio = portfolioHistory.Find(p => p.timestamp == currentDate.Date);
                searchLimit++;
            }

            if (searchLimit > 0)
            {
                StockMonitorLogger.WriteMsg("Current day: " + currentDay.ToString());
                StockMonitorLogger.WriteMsg("AFTER");
                StockMonitorLogger.WriteMsg("Current day: " + currentDate.ToString());
            }
            

            var oldPortfolio = portfolioHistory.Find(p => p.timestamp == oldDate.Date);
            searchLimit = 0;
            while (oldPortfolio == null && searchLimit < 15)
            {
                oldDate = oldDate.AddDays(-1); // if not find on current day, adjust to previous day, 
                oldPortfolio = portfolioHistory.Find(p => p.timestamp == oldDate.Date);
                searchLimit++;
            }

            if (searchLimit > 0)
            {
                StockMonitorLogger.WriteMsg("Old day: " + oldDay.ToString());
                StockMonitorLogger.WriteMsg("AFTER ");
                StockMonitorLogger.WriteMsg("Old day: " + oldDate.ToString());
            }

            if (currentPortfolio == null || oldPortfolio == null)
            {
                StockMonitorLogger.WriteMsg("ERROR, could not find dates to calculate portfolio development, Skipping");
                return 0;
            }

            var currentVal = currentPortfolio.value;
            var oldVal = oldPortfolio.value;

            if (!indexCalculation)
            {
                currentVal += currentPortfolio.wallet;
                oldVal += oldPortfolio.wallet;
            }

            if(oldVal == 0)
            {
                return 0;
            }
            
            var diff = currentVal - oldVal;
            return (diff / oldVal) * 100;
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

            
            oneWeek = CalculateDevelopmentBetweenDates(currentDay.timestamp.AddDays(-7), currentDay.timestamp);
            oneMonth = CalculateDevelopmentBetweenDates(currentDay.timestamp.AddMonths(-1), currentDay.timestamp);
            sixMonths = CalculateDevelopmentBetweenDates(currentDay.timestamp.AddMonths(-6), currentDay.timestamp);
            oneYear = CalculateDevelopmentBetweenDates(currentDay.timestamp.AddYears(-1), currentDay.timestamp);
                       
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


            // breaking any rules
            foreach (var rule in rules)
            {
                switch (rule.rule)
                {
                    // DO not trust MA200 values of 0, since it can be just def value --> not calculated, will filter a few points, but very few i assume
                    case TRule.AboveMa:
                        if (datapoint.MA200 == 0 || datapoint.MA200 < rule.RuleValue)
                        {
                            return false;
                        }
                        break;
                    case TRule.BelowMa:
                        if (datapoint.MA200 == 0 || datapoint.MA200 > rule.RuleValue)
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

            // Buy
            if (datapoint.OwnedCnt == 0) {
                if(ComplyToRules(buyRules, datapoint))
                {
                    var (count, totalCost) = CalculateCost(datapoint, Wallet);
                    Wallet -= totalCost;
                    datapoint.OwnedCnt += count;
                }
            }
            else
            {// SELL
                if (ComplyToRules(sellRules, datapoint))
                {
                    Wallet += datapoint.OwnedCnt * datapoint.Price;
                    datapoint.OwnedCnt = 0;
                }
            }

            // update stock status, from possible actions
            stock.OwnedCnt = datapoint.OwnedCnt;
            //stock.Price = h.Price;
        }

        public void Run()
        {
            // TODO skriva owned cnt i stocklistan, per historik, för att få koll på utvekcling, väldigt lik vanliga procentuella uträkningen också

            // latest hisory is the same as current, dont duplicate
            var stockHistories = simulatorStocks.SelectMany(s => s.History).OrderBy(h => h.Date);
            var oldestStock = stockHistories.FirstOrDefault();
            var newestStock = stockHistories.LastOrDefault();

            ClearOldData();
            

            if (oldestStock== null || newestStock == null)
            {
                StockMonitorLogger.WriteMsg("ABORT, Could not find timestamps, ABORT");
                return;
            }

            // todo, tempcode
            oldestStock.Date = newestStock.Date.AddYears(-1);

            // History när läst från db går från äldsta i 0 --> 27/9, till nyaste sist 14 --> 22/10
            var simulationDay = oldestStock.Date;

 

            while (simulationDay != newestStock.Date.AddDays(1).Date)
            {
                // TODO, behöver jag titta på om en aktie har ägts, men finns inte med längre, (bytt lista eller avnoterats) Som t.ex SAS, om jag hade ägt det, och de avnoterades. vad händer?

                if(simulationDay < DateTime.Parse("2023-10-28"))
                {
                    StockMonitorLogger.WriteMsg("fdsakfda");
                }

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
                Name = "6m %",
                HeaderText = "6m %",
                ToolTipText = "6 months",
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

        private List<Simulation> AddIndexes()
        {
            return new List<Simulation>();

            return new List<Simulation> { 
                new Simulation()
                {
                    stockMarket = TMarket.IndexFirstNorthAll,
                    indexCalculation = true,
                    dividentRequired = false,
                    profitRequired = false,
                    buyRules =
                        {
                            new Rule (TRule.Index)
                        },
                    sellRules =
                        {
                            new Rule(TRule.Never)
                        }
                },
                new Simulation()
                {
                    stockMarket = TMarket.IndexOMXSmallCap,
                    indexCalculation = true,
                    dividentRequired = false,
                    profitRequired = false,
                    buyRules =
                        {
                            new Rule (TRule.Index)
                        },
                    sellRules =
                        {
                            new Rule(TRule.Never)
                        }
                },
                new Simulation()
                {
                    stockMarket = TMarket.IndexOMXMidCap,
                    indexCalculation = true,
                    dividentRequired = false,
                    profitRequired = false,
                    buyRules =
                        {
                            new Rule (TRule.Index)
                        },
                    sellRules =
                        {
                            new Rule(TRule.Never)
                        }
                },
                new Simulation()
                {
                    stockMarket = TMarket.IndexOMXLargeCap,
                    indexCalculation = true,
                    dividentRequired = false,
                    profitRequired = false,
                    buyRules =
                        {
                            new Rule (TRule.Index)
                        },
                    sellRules =
                        {
                            new Rule(TRule.Never)
                        }
                },
                new Simulation()
                {
                    stockMarket = TMarket.IndexOMXSGI,
                    indexCalculation = true,
                    dividentRequired = false,
                    profitRequired = false,
                    buyRules =
                        {
                            new Rule (TRule.Index)
                        },
                    sellRules =
                        {
                            new Rule(TRule.Never)
                        }
                },




            };
        }
        private List<Simulation> generateSimulations()
        {
            List<Simulation> returnSims = AddIndexes();

            returnSims.Add(new Simulation()
            {
                dividentRequired = false,
                profitRequired = false,
                stockMarket = TMarket.AllExceptFirstNorth,
                buyRules =
                {
                    new Rule(TRule.AboveMa, 0),
                    new Rule(TRule.BelowMa, 15),
                },
                sellRules =
                {
                    new Rule(TRule.BelowMa, -5)
                }
            });
        /*    returnSims.Add(new Simulation()
            {
                stockMarket = TMarket.All,
                dividentRequired = false,
                profitRequired = false,
                buyRules =
                {
                    new Rule(TRule.AboveMa, 0),
                    new Rule(TRule.BelowMa, 15),
                },
                sellRules =
                {
                    new Rule(TRule.BelowMa, -5)
                }
            });*/

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
                sim.oneWeek,
                sim.oneMonth,
                sim.sixMonths,
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