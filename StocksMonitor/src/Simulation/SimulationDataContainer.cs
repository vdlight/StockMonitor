
using StocksMonitor.Data.DataStoreNS;
using StocksMonitor.LoggerNS;
using StocksMonitor.Data.StockNS;
using StocksMonitor.Simulation.ConfigurationNS;
using StocksMonitor.Simulation.SimulationNS;
using StocksMonitor.Simulation.DefinitionsNS;

namespace StocksMonitor.Simulation.DataContainerNS
{
#if SIMULATIONS
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

       

        public void UpdateData()
        {
            dataGrid.Rows.Clear(); // Clear existing rows

            simulations.AddRange(SimulationDefinitions.generateSimulations());

            var simCount = simulations.Count(); // TODO, rename to like configuration?
            StocksMonitorLogger.WriteMsg("Running " + simCount + " simulations...");

            var tasks = new List<Task>();

            foreach (var sim in simulations)
            {
                tasks.Add(
                    Task.Run(() => sim.Run(stocks: store.stocks)
                ));
            }

            Task.WhenAll(tasks).Wait();
            simulations.ForEach(s => AddSimulationToDataGrid(s));
            StocksMonitorLogger.WriteMsg("Simulations Done");
        }

        public void AddSimulationToDataGrid(SimulationConfiguration sim)
        {
            DataGridViewRow row = new DataGridViewRow();

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
    #endif
}