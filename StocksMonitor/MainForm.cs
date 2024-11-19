using StocksMonitor.Data.DataStoreNS;
using StocksMonitor.LoggerNS;
using StocksMonitor.StockScreener.DataContainerNS;
using StocksMonitor.StockScreener.VisualizationNS;
using StocksMonitor.Simulation.DataContainerNS;
using StocksMonitor.Simulation.VisualizationNS;
using StocksMonitor.Data.StockNS;

namespace StocksMonitor
{
    public partial class MainForm : Form
    {
        DataStore store = new DataStore();
        private bool startup = true;

#if SIMULATIONS
        private SimulationDataVisualization dataVisualization;
#else
        private StockDataVisualization dataVisualization;
#endif
        private DataContainer dataContainer;

        private List<Stock> multiSelect = new List<Stock>();
        private Stock selectedStock;
        private DataGridViewRow selectedRow;

        public MainForm()
        {
            InitializeComponent();
            BuildMainMenu();

            Ma200limit.Text = "-2";
            Ma200Highlimit.Text = "35";
            overProfit_textbox.Text = "20";
            refillTextbox.Text = "10";

#if DEBUG
            this.Text = "StocksMonitor debug";
            dataContainer = new DataContainer(dataGrid, store);
            dataContainer.init();

            // TODO, react for changes
            dataContainer.SetLimits(
                decimal.Parse(Ma200limit.Text),
                decimal.Parse(Ma200Highlimit.Text),
                decimal.Parse(refillTextbox.Text),
                decimal.Parse(overProfit_textbox.Text)
            );

#elif SIMULATIONS
            this.Text = "StocksMonitor simulations";
            WarningsGroupBox.Visible = false;
            StockFiltersGroupBox.Visible = false;
            dataContainer = new DataContainer(dataGrid, store);
            dataContainer.init();
            dataVisualization = new SimulationDataVisualization(stockChart);

#else
            this.Text = "StocksMonitor " + "RELEASE ";
            dataContainer = new DataContainer(dataGrid, store);
            dataContainer.init();
            dataVisualization = new StockDataVisualization(stockChart);
            fromCalander.Visible = false;
            toCalender.Visible = false;
            addCustomButton.Visible = false;

            // TODO, react for changes
            dataContainer.SetLimits(
                decimal.Parse(Ma200limit.Text),
                decimal.Parse(Ma200Highlimit.Text),
                decimal.Parse(refillTextbox.Text),
                decimal.Parse(overProfit_textbox.Text)
            );
#endif

            StocksMonitorLogger.SetOutput(consoleOutput);
            timer1000.Enabled = true;
            oneMonthRadioButton.Checked = true;
        }

        private void BuildMainMenu()
        {
            menuStrip1 = new MenuStrip();

            // top level menus
            var fileMenu = new ToolStripMenuItem("File");
            var testMenu = new ToolStripMenuItem("Test");
            var simulationMenu = new ToolStripMenuItem("Simulation");

            menuStrip1.Items.Add(fileMenu);
            menuStrip1.Items.Add(simulationMenu);
            menuStrip1.Items.Add(testMenu);

            // sub level menus
            var getStockData = new ToolStripMenuItem("Get stock data");
            fileMenu.DropDownItems.Add(getStockData);

            // sub level menus
            var getOwnedData = new ToolStripMenuItem("Get owned data");
            fileMenu.DropDownItems.Add(getOwnedData);

            var testRun = new ToolStripMenuItem("Test run");
            testMenu.DropDownItems.Add(testRun);

            var simulationRun = new ToolStripMenuItem("Simulation run");
            simulationMenu.DropDownItems.Add(simulationRun);

            // event handlers
            getStockData.Click += new EventHandler(GetStockData_Click);
            getOwnedData.Click += new EventHandler(GetOwnedData_Click);
            testRun.Click += new EventHandler(TestRun_Click);
            simulationRun.Click += new EventHandler(SimulationRun_Click);

            // add menustrip to form
            this.Controls.Add(menuStrip1);
            this.MainMenuStrip = menuStrip1;

#if !DEBUG
            testMenu.Enabled = false;
#endif

#if SIMULATIONS

            //parseData.Enabled = false;
#endif
        }

        private void GetOwnedData_Click(object? sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            store.GetOwnedData();
        }


        // TODO, spara data bara när det är efter kl 8 UTC, (7 swe) för då ska senaste information finnas från API
        private void GetStockData_Click(object? sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            store.UpdateStockDataBD();
        }
        private void SimulationRun_Click(object? sender, EventArgs e)
        {

        }

        private async void TestRun_Click(object? sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }
        }

        private async void timer1000_Tick(object sender, EventArgs e)
        {
            if (startup)
            {
                startup = false;
                // Read Data from database at startup 

                store.Startup();
            }

            timeLabel.Text = $"Time: {StocksMonitorLogger.GetTimeString()}";
        }

        //   TODO, kan jag högerklicka i listan och sätta en, filtrera, och dölj attribut som jag sparar tillsammans med aktien, så att jag lätt kan filtrera vad jag vill navigera emellan i grafen

        private void UpdateStocksView()
        {
            StocksMonitorLogger.WriteMsg("Refreshing data grid");

#if SIMULATIONS
            dataContainer.UpdateData();
#else
            dataContainer.UpdateData(
                warnings: showWarnings_checkbox.Checked, 
                hidden: hiddenCheckBox.Checked,
                wanted: wantedCheckbox.Checked,
                intrested: intrestedCheckBox.Checked,
                owned: ownedCheckBox.Checked);

#endif
            // TODO; möjlighet att välja prcentuell utveckling i graph. bra för jämförelser när listor har så olika priser, också gällande t.ex. index jämf
            UpdateInfotexts();
            StocksMonitorLogger.WriteMsg("Refreshing data grid DONE");
        }

        private void UpdateInfotexts()
        {
#if !SIMULATIONS
            // -1 rows since, empty row to write at the end
            stockListLabel.Text = $"Showing {dataGrid.RowCount - 1} of {store.stocks.Count} pcs";

            Ma_label.Text = $"Ma200:  {dataContainer.GetMa200Warnings}";
            overProfit_label.Text = $"Profit:  {dataContainer.GetOverOverProfitWarnings}";
            refillLabel.Text = $"Refill:  {dataContainer.GetRefillWarnings}";
#endif
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            UpdateStocksView();
        }

        private void intrestedButton_Click(object sender, EventArgs e)
        {
            // the selected stock is master, to set filter on or off for possibly all selected
            var setToStatus = !selectedStock.filters.intrested;
            List<string> names = [];

            foreach (DataGridViewRow row in dataGrid.SelectedRows)
            {
                names.Add(row.Cells["Name"].Value.ToString());
                row.Cells["Intrested"].Value = setToStatus ? "X" : "";
                if (row.Cells["Hidden"].Value == "X")
                {
                    row.Cells["Hidden"].Value = "";
                }
            }

            var filtredStocks = store.stocks.Where(s => names.Contains(s.Name)).ToList();

            foreach (var stock in filtredStocks)
            {
                stock.filters.intrested = setToStatus;

                if (stock.filters.intrested)
                    stock.filters.hidden = false;
            }
            updateStockFilterInformation();
        }
        private void updateStockFilterInformation()
        {
            var selectedColor = Color.Green;
            var defaultColor = SystemColors.Control;

            intrestedButton.BackColor = selectedStock.filters.intrested ? selectedColor : defaultColor;
            hiddenButton.BackColor = selectedStock.filters.hidden ? selectedColor : defaultColor;

            selectedRow.Cells["Hidden"].Value = selectedStock.filters.hidden ? "X" : "";
            selectedRow.Cells["Intrested"].Value = selectedStock.filters.intrested ? "X" : "";
            dataGrid.Focus();
        }

        private void hiddenButton_Click(object sender, EventArgs e)
        {
            var setToStatus = !selectedStock.filters.hidden;
            List<string> names = [];

            foreach (DataGridViewRow row in dataGrid.SelectedRows)
            {
                names.Add(row.Cells["Name"].Value.ToString());
                row.Cells["Hidden"].Value = setToStatus ? "X" : "";
                if (row.Cells["Intrested"].Value == "X")
                {
                    row.Cells["Intrested"].Value = "";
                }
            }

            var filtredStocks = store.stocks.Where(s => names.Contains(s.Name)).ToList();

            foreach (var stock in filtredStocks)
            {
                stock.filters.hidden = setToStatus;

                if (stock.filters.hidden)
                    stock.filters.intrested = false;
            }

            updateStockFilterInformation();
        }

        private void clearHiddenButton_Click(object sender, EventArgs e)
        {
            foreach (Stock stock in store.stocks)
            {
                stock.filters.hidden = false;
            }
            updateStockFilterInformation();
        }
        private void clearInterested_Click(object sender, EventArgs e)
        {
            foreach (Stock stock in store.stocks)
            {
                stock.filters.intrested = false;
            }
            updateStockFilterInformation();
        }
        private void clearAll_Click(object sender, EventArgs e)
        {
            foreach (Stock stock in store.stocks)
            {
                stock.filters.intrested = false;
                stock.filters.hidden = false;
            }
            updateStockFilterInformation();
        }
        private void dataGrid_SelectionChanged(object sender, EventArgs e)
        {
            string? name;

            selectedLabel.Text = "Selected " + dataGrid.SelectedRows.Count.ToString() + " pcs";

            if (dataGrid.SelectedRows.Count > 0) // Ensure at least one row is selected
            {
                selectedRow = dataGrid.SelectedRows[0]; // Get the first selected row

                if (selectedRow.Cells["Name"].Value == null) // Ensure the cell contains a value
                {
                    return;
                }

                try
                {
                    name = selectedRow.Cells["Name"].Value.ToString();
                    if (name == null)
                    {
                        StocksMonitorLogger.WriteMsg("WARNING: could not read name of selected stock");
                        return;
                    }

                    selectedStock = store.stocks.Single(s => s.Name == name);
                    StockFiltersGroupBox.Text = selectedStock.Name;
                    Clipboard.SetText(name);

                    updateStockFilterInformation();

                    List<string> selectedNames = [];

                    foreach (DataGridViewRow row in dataGrid.SelectedRows)
                    {
                        selectedNames.Add(row.Cells["Name"].Value.ToString());
                    }
                    dataVisualization.SelectedRows(selectedNames, store.stocks, oneYearRadioButton.Checked);
                }
                catch (Exception exception)
                {
                    StocksMonitorLogger.WriteMsg("ERROR: datagrid_SelectionCHanged " + exception.Message);
                }
            }
        }
        private void oneMonthRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            dataGrid_SelectionChanged(sender, e);
        }

        private void oneYearRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            dataGrid_SelectionChanged(sender, e);
        }

        private void addCustomButton_Click(object sender, EventArgs e)
        {

        }
    }
}
