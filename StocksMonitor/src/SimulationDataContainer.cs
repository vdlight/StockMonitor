using Microsoft.EntityFrameworkCore.Metadata.Conventions;
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
                Name = "1 day %",
                HeaderText = "1 day %",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "1 week %",
                HeaderText = "1 week %",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "1 month %",
                HeaderText = "1 Month %",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "1 Year %",
                HeaderText = "1 year",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Investment",
                HeaderText = "Investment ",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Value",
                HeaderText = "Value",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Wallet",
                HeaderText = "Wallet",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Tot development %",
                HeaderText = "Tot development %",
                ValueType = typeof(decimal)
            });

            // Set properties for better appearance
            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                if (i == 0)
                {
                    // name shall be leftaligned, rest, center
                    dataGrid.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    continue;
                }
                dataGrid.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        public void UpdateData(bool warnings, bool hidden, bool wanted, bool intrested, bool owned)
        {
        }
        public void UpdateData()
        {
            dataGrid.Rows.Clear(); // Clear existing rows
            

            var simu1 = new Simulation(store.stocks, new Strat_BuyAndHold_NoMA());
            simu1.Run();
            AddSimulationToDataGrid(simu1);

            var sim2 = new Simulation(store.stocks, new Strat_BuyWithinMA15_AndAdjust());
            sim2.Run();
            AddSimulationToDataGrid(simu1);


        }
        public void AddSimulationToDataGrid(Simulation sim)
        {
            DataGridViewRow row = new DataGridViewRow();
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
        const decimal originalInvestment = 100000;
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
        public Simulation(List<Stock> stocks, Strategy strategy)
        {
            simulatorStocks.AddRange(stocks);
            name = "TEST";
            AddToWallet(originalInvestment);
            this.strategy = strategy;
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
            ClearOldData();

            if (oldestTimeStamp == null)
            {
                StockMonitorLogger.WriteMsg("ABORT, Could not find oldest timestamp, ABORT");
                return;
            }
            // History när läst från db går från äldsta i 0 --> 27/9, till nyaste sist 14 --> 22/10
            var simulationDay = oldestTimeStamp.Date;

            while (simulationDay != DateTime.Now.AddDays(1).Date)
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

            if (historyDays == null)
            {
                StockMonitorLogger.WriteMsg("warning, could not find history day when sum of day is calculated");
                return 0;
            }

            // TODO should wallet be in here
            return historyDays.Sum(h => h.OwnedCnt * h.Price) + Wallet;
        }

        public void CalculateSimulationResult()
        {
            portfolioHistory.Reverse();

            var firstDay = portfolioHistory.First();

            foreach(var history in portfolioHistory)
            {
                var timespan = history.timestamp - firstDay.timestamp;

                if (oneDay == 0 && timespan.Days >= 1)
                {
                    oneDay = (SumPortfolioDay(history) / history.investment) * 100;
                }
                else if (oneWeek == 0 && timespan.Days >= 7)
                {
                    oneWeek = (SumPortfolioDay(history) / history.investment) * 100;
                }
                else if (oneMonth == 0 && timespan.Days > 31)
                {
                    oneMonth = (SumPortfolioDay(history) / history.investment) * 100;
                }
                else if (oneYear == 0 && timespan.Days >= 365)
                {
                    oneYear = (SumPortfolioDay(history) / history.investment) * 100;
                }
            }
            TotDevelopment = ((Wallet + Value) / Investment) * 100;
        }
    }
}
#endif