using GrapeCity.DataVisualization.Chart;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using StocksMonitor.src.databaseWrapper;
using StocksMonitor.src.dataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


#if SIMULATIONS
namespace StocksMonitor.src
{

    public class DataContainer
    {
        private DataGridView dataGrid;
        private DataStore store;
        private int investmentTarget = 500; // TODO, make adjustable
 
        
        public DataContainer(DataGridView dataGridView, DataStore store)
        {
            this.dataGrid = dataGridView;
            this.store = store;
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
        public void UpdateData()
        {
            dataGrid.Rows.Clear(); // Clear existing rows

            Stack<string> markets = new ();
            markets.Push("Large Cap Stockholm");
            markets.Push("Mid Cap Stockholm");
            markets.Push("Small Cap Stockholm");
            markets.Push("First North Stockholm");

            List<Simulation> simulations =
            [
                new Simulation(store.stocks.Where(s => s.List != "First North Stockholm").ToList(), new StockDevelopmentSimulation(investmentTarget, 20, "Stockholm stock market"), noWallet: true),
                new Simulation(store.stocks, new StockDevelopmentSimulation(investmentTarget, 20, "Stockholm including First north"), noWallet: true),
                new Simulation(store.stocks.Where(s => s.List == markets.Peek()).ToList(), new StockDevelopmentSimulation(investmentTarget, 20, markets.Pop()), noWallet: true),
                new Simulation(store.stocks.Where(s => s.List == markets.Peek()).ToList(), new StockDevelopmentSimulation(investmentTarget, 20, markets.Pop()),noWallet: true),
                new Simulation(store.stocks.Where(s => s.List == markets.Peek()).ToList(), new StockDevelopmentSimulation(investmentTarget, 20, markets.Pop()), noWallet: true),
                new Simulation(store.stocks.Where(s => s.List == markets.Peek()).ToList(), new StockDevelopmentSimulation(investmentTarget, 20, markets.Pop()), noWallet: true),


                
                new Simulation(store.stocks, new Strat_BuyAndHold_NoMA(investmentTarget, 20)),
                new Simulation(store.stocks, new Strat_BuyWithinMA0And15_AndHold(investmentTarget, 20))
            ];


            foreach(var sim in simulations)
            {
                sim.Run();
                AddSimulationToDataGrid(sim);
            }


            var stocks = store.stocks.Where(s => s.List == "Large Cap Stockholm")
                .Where(h => h != null);

            var FN_base = stocks.Select(s => s.History.First()).Sum(s => s.Price);
            var FN_End = stocks.Select(s => s.History.Last()).Sum(s => s.Price);

            var diff = FN_End - FN_base;

            StockMonitorLogger.WriteMsg("Large cap, base: " + FN_base);
            StockMonitorLogger.WriteMsg("Large cap, base / cnt: " + FN_base / stocks.Count());
            StockMonitorLogger.WriteMsg("Large cap, End : " + FN_End);
            StockMonitorLogger.WriteMsg("Large cap, diff: " + diff);
            StockMonitorLogger.WriteMsg("Large cap, diff %: " + diff/ FN_base *100);

            var Stock_base = stocks.FirstOrDefault()?.History.FirstOrDefault()?.Price;
            var Stock_end = stocks.FirstOrDefault()?.History.LastOrDefault()?.Price;


            var stock_diff = Stock_end - Stock_base;

            StockMonitorLogger.WriteMsg("First stock first " + stocks.FirstOrDefault()?.Name + " " + Stock_base);
            StockMonitorLogger.WriteMsg("First stock last " + stocks.FirstOrDefault()?.Name + " " + + Stock_end);
            StockMonitorLogger.WriteMsg("First stock, diff: " + stock_diff);
            StockMonitorLogger.WriteMsg("First stock, diff %: " + stock_diff / Stock_base * 100);


            // Print the results
            StockMonitorLogger.WriteMsg("Base date: " + store.stocks[0].History.FirstOrDefault()?.Date);
            StockMonitorLogger.WriteMsg("End date: " + store.stocks[0].History.LastOrDefault()?.Date);

        }


        public void AddSimulationToDataGrid(Simulation sim)
        {
            DataGridViewRow row = new DataGridViewRow();
            
            //var stockDevelopement = sim.strategy.GetType() == typeof(StockDevelopmentSimulation);

            row.CreateCells(dataGrid,
                sim.name,
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
        decimal wallet;
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

    public class Simulation
    {
        readonly decimal originalInvestment = 100000;
        const decimal investmentTarget = 500;

        List<Stock> simulatorStocks = [];
        public string name { get; private set; }
        public decimal oneDay { get; private set; } = 0;
        public decimal oneWeek { get; private set; } = 0;
        public decimal oneMonth { get; private set; } = 0;
        public decimal oneYear { get; private set; } = 0;
        public decimal Investment { get; private set; } = 0;
        public decimal Value { get; private set; } = 0;
        public decimal Wallet { get; private set; } = 0;
        public decimal TotDevelopment { get; private set; } = 0;
    
        private void AddToWallet(decimal value)
        {
            Wallet += value;
            Investment += value;
        } 
        public Simulation(List<Stock> stocks, Strategy strategy, bool noWallet = false)
        {
            simulatorStocks.AddRange(stocks);
            if (!noWallet) {
                AddToWallet(originalInvestment);
            }
            this.strategy = strategy;
            this.name = strategy.Name;
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

        // strategies
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
                        // copy latest status, to have as base for decision
                        h.OwnedCnt = stock.OwnedCnt;

                        // calculate actions
                        var datapointResult = strategy.DetermineAction(dataPoint: h, wallet: Wallet);
                        switch (datapointResult.action)
                        {
                            case StratAction.BUY: 
                                Wallet -= datapointResult.value;
                                h.OwnedCnt += datapointResult.adjustmentCount;
                                break;
                            case StratAction.SELL:
                                // fall-through
                            case StratAction.ADJ_DOWN:
                                Wallet += datapointResult.value;
                                h.OwnedCnt -= datapointResult.adjustmentCount;
                                break;
                            case StratAction.NONE:
                                break;
                        }

                        // update stock status, from possible actions
                        stock.OwnedCnt = h.OwnedCnt; 
                    }
                }
                var allStocksHistoryDay = simulatorStocks.SelectMany(s=> s.History).Where(h=> h.Date == simulationDay);

                if (allStocksHistoryDay == null)
                {
                    StockMonitorLogger.WriteMsg("WARNING, could not find any history data for date " + simulationDay);
                } 
                else
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
            
            
            Value = portfolioHistory.First().value;
            CalculateSimulationResult();
        }

        private decimal SumPortfolioDay(Portfolio day)
        {
            var ownedStock = simulatorStocks.Where(s => s.OwnedCnt > 0);
            var historyDays = ownedStock.SelectMany(s => s.History).Where(h => h.Date == day.timestamp);

            if (! historyDays.Any())
            {
                StockMonitorLogger.WriteMsg("warning, could not find history day when sum of day is calculated");
                return 0;
            }
            // TODO, be able to select start and stop date, not only have the endpoints 

            // TODO should wallet be in here
            return historyDays.Sum(h => h.OwnedCnt * h.Price) + Wallet;
        }

        private decimal CalculateDevelopmentPercentage(Portfolio startPoint, decimal endValue)
        {
            var startValue = SumPortfolioDay(startPoint);
            var diff = endValue - startValue;

            return diff / startValue * 100;
        }
        public void CalculateSimulationResult()
        {
            portfolioHistory.Reverse();

            var currentDay = portfolioHistory.First();
            var currentValue = SumPortfolioDay(currentDay);
            TotDevelopment = CalculateDevelopmentPercentage(portfolioHistory.LastOrDefault(), currentValue);

            foreach (var history in portfolioHistory)
            {
                var timespan = currentDay.timestamp - history.timestamp;

                if (oneDay == 0 && timespan.TotalDays >= 1)
                {
                    oneDay = CalculateDevelopmentPercentage(history, currentValue);
                }
                else if (oneWeek == 0 && timespan.TotalDays >= 7)
                {
                    oneWeek = CalculateDevelopmentPercentage(history, currentValue);
                }
                else if (oneMonth == 0 && timespan.TotalDays > 31)
                {
                    oneMonth = CalculateDevelopmentPercentage(history, currentValue);
                }
                else if (oneYear == 0 && timespan.TotalDays >= 365)
                {
                    oneYear = CalculateDevelopmentPercentage(history, currentValue);
                }
            }
        }
    }
}
#endif