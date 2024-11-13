using GrapeCity.DataVisualization.Chart;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Identity.Client;
using StocksMonitor.src.databaseWrapper;
using StocksMonitor.src.dataStoreNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;


#if SIMULATIONS
namespace StocksMonitor.src.Simulation
{
    public class DataContainer
    {
        private DataGridView dataGrid;
        private DataStore store;
        private int investmentTarget = 500; // TODO, make adjustable

        private List<SimulationConfiguration> simulations;

        private static readonly Dictionary<TMarket, string> markets = new Dictionary<TMarket, string>
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


        public DataContainer(DataGridView dataGridView, DataStore store)
        {
            dataGrid = dataGridView;
            this.store = store;
            simulations = new();
        }

        public void init()
        {
            dataGrid.Columns.Clear(); // Clear any existing columns

            // Define your columns
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Name",
                ValueType = typeof(string)
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
                Name = "2y %",
                HeaderText = " 2y %",
                ToolTipText = "2 year",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "5y %",
                HeaderText = " 5y %",
                ToolTipText = "5 year",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "10y %",
                HeaderText = " 10 y %",
                ToolTipText = "10 year",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "15y %",
                HeaderText = "15y %",
                ToolTipText = "15 years",
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

        private List<SimulationConfiguration> AddIndexes()
        {

            return new List<SimulationConfiguration> {   

                new SimulationConfiguration()
                {
                    stockMarket = TMarket.IndexOMXSGI,
                    configuration =
                    {
                        indexCalculation = true,
                        dividentRequired = false,
                        profitRequired = false,
                        buyRules =
                        {
                            new Rule (TRule.None)
                        },
                        sellRules =
                        {
                            new Rule(TRule.Never)
                        }
                    }

                }
            };
        }
        private List<SimulationConfiguration> generateSimulations()
        {
            List<SimulationConfiguration> returnSims = AddIndexes();
            // TODO, Möjlighet att i simuleringar, välja vilka markander som ska köras. Så kör jag alla varianter för vald marknad sedan
            TMarket[] selectedMarkets = {
                TMarket.All, TMarket.AllExceptFirstNorth
            };

            foreach (var market in selectedMarkets)
            {
                for(int balance = 0; balance <= 1; balance ++)
                { 
                    for (int profitRequired = 0; profitRequired <= 1; profitRequired++)
                    {
                        for (int divident = 0; divident <= 1; divident++)
                        {
                            // buy within ma limits. never adjust, sell
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.AboveMa, 0),
                                        new Rule(TRule.BelowMa, 15),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.BelowMa, -5)
                                    }
                                },
                                stockMarket = market,
                            });
                            // buy within ma limits. never adjust, never sell
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.AboveMa, 0),
                                        new Rule(TRule.BelowMa, 15),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.None)
                                    }
                                },
                                stockMarket = market,
                            });
                            // buy and keep, never adjust
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.None),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.Never)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.None)
                                    }
                                },
                                stockMarket = market,
                            });
                            // buy within limits, and rebuy, sell below -5
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.None),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.AboveMa, 0),
                                        new Rule(TRule.BelowMa, 15)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.BelowMa, -5)
                                    }
                                },
                                stockMarket = market,
                            });
                            // buy within limits, and rebuy, never sell
                            returnSims.Add(new SimulationConfiguration()
                            {
                                configuration = {
                                    dividentRequired = divident == 1,
                                    profitRequired = profitRequired == 1,
                                    balanceInvestment = balance == 1,
                                    buyRules =
                                    {
                                        new Rule(TRule.None),
                                    },
                                    adjustBuyRules =
                                    {
                                        new Rule(TRule.AboveMa, 0),
                                        new Rule(TRule.BelowMa, 15)
                                    },
                                    sellRules =
                                    {
                                        new Rule(TRule.Never)
                                    }
                                },
                                stockMarket = market,
                            });
                        }
                    }
                }
            }
            return returnSims;
            // TODO, kan generera namn, från TOString() eller så, för objekten, så de genereras. När datacolum skivs
        }


        public void UpdateData()
        {
            dataGrid.Rows.Clear(); // Clear existing rows

            simulations.AddRange(generateSimulations());

            var simCount = simulations.Count(); // TODO, rename to like configuration?
            StockMonitorLogger.WriteMsg("Running " + simCount + " simulations...");

            var tasks = new List<Task>();

            foreach (var sim in simulations)
            {
                tasks.Add(
                    Task.Run(() => sim.Run(stocks: store.stocks)
                ));
            }

            Task.WhenAll(tasks).Wait();
            simulations.ForEach(s => AddSimulationToDataGrid(s));
            StockMonitorLogger.WriteMsg("Simulations Done");
        }


        public void AddSimulationToDataGrid(SimulationConfiguration sim)
        {
            DataGridViewRow row = new DataGridViewRow();

            //var stockDevelopement = sim.strategy.GetType() == typeof(StockDevelopmentSimulation);

            row.CreateCells(dataGrid,
                sim.getNameString(),
                sim.oneMonth,
                sim.sixMonths,
                sim.oneYear,
                sim.twoYears,
                sim.fiveYears,
                sim.tenYears,
                sim.fifteenYears,
                sim.Investment,
                sim.Value,
                sim.Wallet
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
            timestamp = date;
            this.wallet = wallet;
            this.investment = investment;
            this.value = value;
        }
    }
}
#endif