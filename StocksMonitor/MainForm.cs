
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using StocksMonitor.src.databaseWrapper;
using StocksMonitor.src.dataStore;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using StocksMonitor.src;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.DataVisualization.Charting;

namespace StocksMonitor
{
    public partial class MainForm : Form
    {
        DataStore store = new DataStore();
        private bool startup = true;
        private string[] columns = { "Name", "Price", "MA200 %", "Owned", "Earned %", "Hidden", "Interested", "MultiChart" };
        private DataVisualization dataVisualization;
        private int underMa200Warning = 0;
        private int overProfitWarning = 0;
        private int overLossLimitWarning = 0;

        private int investmentTarget = 500; // TODO, make adjustable

        private List<Stock> multiSelect = new List<Stock>();
        private Stock selectedStock;
        private DataGridViewRow selectedRow;

        public MainForm()
        {
            InitializeComponent();
            BuildMainMenu();

            dataGrid.Columns.Clear(); // Clear any existing columns

            // Define your columns
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Price",
                ValueType = typeof(String)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Price",
                HeaderText = "Price",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MA200",
                HeaderText = "MA200 %",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Owned",
                HeaderText = "Owned",
                ValueType = typeof(int)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Earned",
                HeaderText = "Earned %",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Hidden",
                HeaderText = "Hidden",
                ValueType = typeof(string)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Intrested",
                HeaderText = "Intrested",
                ValueType = typeof(string)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MultiChart",
                HeaderText = "MultiChart",
                ValueType = typeof(string)
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


            undeMa200limit.Text = "0";
            overProfit_textbox.Text = "20";
            overLossTextbox.Text = "10";

            StockMonitorLogger.SetOutput(consoleOutput);
            timer1000.Enabled = true;

            dataVisualization = new DataVisualization(stockChart);

            oneMonthRadioButton.Checked = true;


#if DEBUG
            this.Text = "StockMonitor debug";
#else
            this.Text = "StockMonitor " + "RELEASE ";
#endif
        }


        private void BuildMainMenu()
        {
            menuStrip1 = new MenuStrip();

            // top level menus
            var fileMenu = new ToolStripMenuItem("File");
            var testMenu = new ToolStripMenuItem("Test");

            menuStrip1.Items.Add(fileMenu);
            menuStrip1.Items.Add(testMenu);

            // sub level menus
            var parseData = new ToolStripMenuItem("Parse");
            fileMenu.DropDownItems.Add(parseData);

            var testRun = new ToolStripMenuItem("Test run");
            testMenu.DropDownItems.Add(testRun);


            // event handlers
            parseData.Click += new EventHandler(ParseData_Click);
            testRun.Click += new EventHandler(TestRun_Click);

            // add menustrip to form
            this.Controls.Add(menuStrip1);
            this.MainMenuStrip = menuStrip1;

#if RELEASE
            // disable test, when running against master
            testMenu.Enabled = false;
#endif
        }

        private void ParseData_Click(object? sender, EventArgs e)
        {
            if (sender == null)

            {
                return;
            }

            var result = MessageBox.Show("Do you want to fetch avanza data and write to DB", "Prod DB overwrite", MessageBoxButtons.OKCancel);

            if (result == DialogResult.Cancel)
            {
                StockMonitorLogger.WriteMsg("Skipping parsing data and writing to DB");
                return;
            }

            // this task is async, but dont bother wait here in gui thread, since not dependent on all data
            Task.Run(async () => await
                    store.FetchDataFromAvanza()
            );

        }
        private async void TestRun_Click(object? sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }
#if DEBUG
            StockMonitorLogger.WriteMsg("Test Start");
            // Test
            await store.TestRun();

            TestWarningCalculations();
            StockMonitorLogger.WriteMsg("Test End");
#endif
        }

        private async void timer1000_Tick(object sender, EventArgs e)
        {
            if (startup)
            {
                startup = false;
                // Read Data from database at startup 
                await store.ReadFromDB();
            }

            timeLabel.Text = $"Time: {StockMonitorLogger.GetTimeString()}";
        }

        //   TODO, kan jag högerklicka i listan och sätta en, filtrera, och dölj attribut som jag sparar tillsammans med aktien, så att jag lätt kan filtrera vad jag vill navigera emellan i grafen

        private bool MA200Warning(Stock stock)
        {
            decimal MA200 = stock.MA200;
            decimal MA200Limit = decimal.Parse(undeMa200limit.Text);
            if (MA200 < MA200Limit)
            {
                return true;
            }
            return false;
        }
        private bool OverProfitWarning(Stock stock)
        {
            if (stock.OwnedCnt == 0)
                return false;

            decimal value = (stock.Price) * stock.OwnedCnt;
            decimal valueIfSoldOne = value - stock.Price;
            decimal minValue = investmentTarget + decimal.Parse(overProfit_textbox.Text) / 100 * investmentTarget;
            if (valueIfSoldOne > investmentTarget &&
                (value > minValue))
            {
                return true;
            }
            return false;
        }
        private bool OverLossWarning(Stock stock)
        {
            if (stock.OwnedCnt == 0)
                return false;

            bool loss = stock.Price < stock.PurPrice;
            decimal lostValue = ((stock.PurPrice - stock.Price) / stock.PurPrice) * 100; // Using PurPrice for correct calculation
            decimal loss_limit = decimal.Parse(overLossTextbox.Text);
            if (loss & (lostValue > loss_limit))
            {
                return true;
            }
            return false;
        }
        private decimal CalculateEarning(Stock stock)
        {
            decimal earned;
            if (stock.OwnedCnt > 0)
            {
                earned = Math.Round(((stock.Price - stock.PurPrice) / stock.PurPrice) * 100, 2);
            }
            else
            {
                earned = 0;
            }

            return earned;
        }

        private void UpdateStocksView()
        {
            StockMonitorLogger.WriteMsg("Refreshing data grid");

            dataGrid.Rows.Clear(); // Clear existing rows

            //+ CalculateWarnings();

            foreach (var stock in store.stocks)
            {
                // Check filters, skip if
                if (
                    (showWarnings_checkbox.Checked && !stock.filters.warning) ||
                    (hiddenCheckBox.Checked && stock.filters.hidden) ||
                    (wantedCheckbox.Checked && stock.OwnedCnt > 0) ||
                    (intrestedCheckBox.Checked && !stock.filters.intrested) ||
                    (ownedCheckBox.Checked && stock.OwnedCnt == 0))
                {
                    continue;
                }

                // Add a new row with the stock data
                decimal earned = CalculateEarning(stock);

                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGrid,
                    stock.Name,
                    stock.Price,
                    stock.MA200,
                    stock.OwnedCnt,
                    earned,
                    stock.filters.hidden ? "X" : "",
                    stock.filters.intrested ? "X" : "",
                    stock.filters.multiChart ? "X" : "");
                dataGrid.Rows.Add(row);


                //private string[] columns = { "Name", "Price", "MA200", "Owned", "Earned", "Hidden", "Interested", "MultiChart" };
                if (MA200Warning(stock))
                {
                    stock.filters.warning = true;
                    underMa200Warning++;
                    row.Cells[2].Style.BackColor = Color.LightSalmon;
                }
                if (OverProfitWarning(stock))
                {
                    stock.filters.warning = true;
                    overProfitWarning++;
                    row.Cells[1].Style.BackColor = Color.LightGreen;
                }
                else if (OverLossWarning(stock))
                {
                    stock.filters.warning = true;
                    overLossLimitWarning++;
                    row.Cells[1].Style.BackColor = Color.Red;
                }
            }
            // TODO, write testcase to confirm order of columns

            UpdateInfotexts();
            StockMonitorLogger.WriteMsg("Refreshing data grid DONE");
        }

        private void UpdateInfotexts()
        {
            // -1 rows since, empty row to write at the end
            stockListLabel.Text = $"Showing {dataGrid.RowCount - 1} of {store.stocks.Count} pcs";

            underMa_label.Text = $"Ma200:  {underMa200Warning}";
            overProfit_label.Text = $"Profit:  {overProfitWarning}";
            overLossLabel.Text = $"Loss:  {overLossLimitWarning}";
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            UpdateStocksView();
        }

        private void intrestedButton_Click(object sender, EventArgs e)
        {
            selectedStock.filters.intrested = !selectedStock.filters.intrested;

            if (selectedStock.filters.intrested)
            {
                selectedStock.filters.hidden = false;
            }

            updateStockFilterInformation();
        }
        private void updateStockFilterInformation()
        {
            var selectedColor = Color.Green;
            var defaultColor = SystemColors.Control;

            intrestedButton.BackColor = selectedStock.filters.intrested ? selectedColor : defaultColor;
            hiddenButton.BackColor = selectedStock.filters.hidden ? selectedColor : defaultColor;
            multiChartButton.BackColor = selectedStock.filters.multiChart ? selectedColor : defaultColor;

            selectedRow.Cells["Hidden"].Value = selectedStock.filters.hidden ? "X" : "";
            selectedRow.Cells["Intrested"].Value = selectedStock.filters.intrested ? "X" : "";
            selectedRow.Cells["MultiChart"].Value = selectedStock.filters.multiChart ? "X" : "";
            dataGrid.Focus();
        }

        private void hiddenButton_Click(object sender, EventArgs e)
        {
            selectedStock.filters.hidden = !selectedStock.filters.hidden;
            if (selectedStock.filters.hidden)
            {
                selectedStock.filters.intrested = false;
            }

            updateStockFilterInformation();
        }

        private void multiChartButton_Click(object sender, EventArgs e)
        {
            selectedStock.filters.multiChart = !selectedStock.filters.multiChart;

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
                    selectedStock = store.stocks.Single(s => s.Name == name);
                    StockFiltersGroupBox.Text = selectedStock.Name;
                    updateStockFilterInformation();

                    List<string> selectedNames = [];

                    foreach (DataGridViewRow row in dataGrid.SelectedRows)
                    {
                        selectedNames.Add(row.Cells["Name"].Value.ToString());
                    }
                    dataVisualization.SelectedRows(selectedNames, store.stocks, oneWeekRadioButton.Checked);
                }
                catch (Exception exception)
                {
                    StockMonitorLogger.WriteMsg("ERROR: " + exception.Message);
                }
            }
        }
        private void oneMonthRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            dataGrid_SelectionChanged(sender, e);
        }

        private void oneWeekRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            dataGrid_SelectionChanged(sender, e);
        }
    }

#if DEBUG
        private void TestWarningCalculations()
        {
            // clear stocks data in store
            store.stocks = [];

            undeMa200limit.Text = "0";
            overProfit_textbox.Text = "20";
            overLossTextbox.Text = "10";

            // normal, no warnings
            store.stocks.Add(new Stock()
            {
                Name = "ABB",
                Price = 100.0m,
                PurPrice = 95.0m,   // 5 kr profit / stock
                OwnedCnt = 2,
                MA200 = 10.2m,      // Price 10.2 % over MA
            });
            store.stocks.Add(new Stock()
            {
                Name = "SAAB",
                Price = 100.2m,
                PurPrice = 110.4m,   // 10 kr loss / stock --> not enough loss
                OwnedCnt = 2,
                MA200 = 0.2m,      // Price 0.2 % over MA
            });
            store.stocks.Add(new Stock()
            {
                Name = "Investor",
                Price = 110.4m, // Total value = 110.4 * 5 --> 552, not enough to start sell
                PurPrice = 91,   // 19.4 kr profit / stock --> enoght profit to sell one
                OwnedCnt = 5, //    21 % profit
                MA200 = 0.2m,      // Price 0.2 % over MA
            });
            UpdateStocksView();

            CompareInt(underMa200Warning, 0);
            CompareInt(overProfitWarning, 0);
            CompareInt(overLossLimitWarning, 0);
            CompareBool(store.stocks[0].filters.warning, false);
            CompareBool(store.stocks[1].filters.warning, false);
            CompareBool(store.stocks[2].filters.warning, false);

            store.stocks.Add(new Stock()
            {
                Name = "Securitas",
                Price = 110.4m, // Total value = 110.4 * 5 --> 552, not enough to start sell
                PurPrice = 91,   // 19.4 kr profit / stock --> enoght profit to sell one
                OwnedCnt = 5, //    21 % profit
                MA200 = -0.2m,      // Price 0.2 % under MA
            });
            UpdateStocksView();

            CompareInt(underMa200Warning, 1);
            CompareInt(overProfitWarning, 0);
            CompareInt(overLossLimitWarning, 0);
            CompareBool(store.stocks[0].filters.warning, false);
            CompareBool(store.stocks[1].filters.warning, false);
            CompareBool(store.stocks[2].filters.warning, false);
            CompareBool(store.stocks[3].filters.warning, true);

            undeMa200limit.Text = "-1"; // lower level of MA limit, so that warning shall not trigg, 1 percentage allowed under MA
            UpdateStocksView();

            CompareInt(underMa200Warning, 0);
            CompareInt(overProfitWarning, 0);
            CompareInt(overLossLimitWarning, 0);
            CompareBool(store.stocks[0].filters.warning, false);
            CompareBool(store.stocks[1].filters.warning, false);
            CompareBool(store.stocks[2].filters.warning, false);
            CompareBool(store.stocks[3].filters.warning, false);


            store.stocks.Add(new Stock()
            {
                Name = "Addvice",
                Price = 121.1m, // Total value = 121.1 * 5 --> , 605.5 enough to sell, but cant sell, because will be less than incestment
                PurPrice = 91,   // 30,1  kr profit / stock --> enoght profit to sell one
                OwnedCnt = 5, //    21 % profit
                MA200 = 0.2m,      // Price 0.2 % over MA
            });
            UpdateStocksView();

            CompareInt(underMa200Warning, 0);
            CompareInt(overProfitWarning, 0);
            CompareInt(overLossLimitWarning, 0);
            CompareBool(store.stocks[0].filters.warning, false);
            CompareBool(store.stocks[1].filters.warning, false);
            CompareBool(store.stocks[2].filters.warning, false);
            CompareBool(store.stocks[3].filters.warning, false);
            CompareBool(store.stocks[4].filters.warning, false);


            store.stocks.Add(new Stock()
            {
                Name = "Addtech",
                Price = 126.3m, // Total value = 126.3 * 5 --> , 631.5 enough to sell, and can sell of one 126, and still keep above investment
                PurPrice = 91,   // 30,1  kr profit / stock --> enoght profit to sell one
                OwnedCnt = 5, //    21 % profit
                MA200 = 0.2m,      // Price 0.2 % over MA
            });
            UpdateStocksView();

            CompareInt(underMa200Warning, 0);
            CompareInt(overProfitWarning, 1);
            CompareInt(overLossLimitWarning, 0);
            CompareBool(store.stocks[0].filters.warning, false);
            CompareBool(store.stocks[1].filters.warning, false);
            CompareBool(store.stocks[2].filters.warning, false);
            CompareBool(store.stocks[3].filters.warning, false);
            CompareBool(store.stocks[4].filters.warning, false);
            CompareBool(store.stocks[5].filters.warning, true);


            store.stocks.Add(new Stock()
            {
                Name = "SBB",
                Price = 98,      // Total value = 99 * 5 --> , 495, loss of 55, with is over 10% of talatl investment
                PurPrice = 110,   // 11 kr loss per stock
                OwnedCnt = 5,     //    total investement 550
                MA200 = 0.2m,      // Price 0.2 % over MA
            });
            UpdateStocksView();

            CompareInt(underMa200Warning, 0);
            CompareInt(overProfitWarning, 1);
            CompareInt(overLossLimitWarning, 1);
            CompareBool(store.stocks[0].filters.warning, false);
            CompareBool(store.stocks[1].filters.warning, false);
            CompareBool(store.stocks[2].filters.warning, false);
            CompareBool(store.stocks[3].filters.warning, false);
            CompareBool(store.stocks[4].filters.warning, false);
            CompareBool(store.stocks[5].filters.warning, true);
            CompareBool(store.stocks[6].filters.warning, true);

            store.stocks.Add(new Stock()
            {
                Name = "SEB",
                Price = 98,      // Total value = 99 * 5 --> , 495, loss of 55, with is over 10% of talatl investment
                PurPrice = 110,   // 11 kr loss per stock
                OwnedCnt = 5,     //    total investement 550
                MA200 = -1.2m,      // Price under MA limit aswell now
            });
            UpdateStocksView();

            CompareInt(underMa200Warning, 1);
            CompareInt(overProfitWarning, 1);
            CompareInt(overLossLimitWarning, 2);
            CompareBool(store.stocks[0].filters.warning, false);
            CompareBool(store.stocks[1].filters.warning, false);
            CompareBool(store.stocks[2].filters.warning, false);
            CompareBool(store.stocks[3].filters.warning, false);
            CompareBool(store.stocks[4].filters.warning, false);
            CompareBool(store.stocks[5].filters.warning, true);
            CompareBool(store.stocks[6].filters.warning, true);
            CompareBool(store.stocks[7].filters.warning, true);

        }

        private void CompareInt(int lhs, int rhs)
        {
            if (lhs != rhs)
            {
                StockMonitorLogger.WriteMsg($"ERROR: {lhs} and {rhs} are not as expected");
            }
        }
        private void CompareBool(bool lhs, bool rhs)
        {
            if (lhs != rhs)
            {
                StockMonitorLogger.WriteMsg($"ERROR: {lhs} and {rhs} are not as expected");
            }
        }
    }

#endif
}


