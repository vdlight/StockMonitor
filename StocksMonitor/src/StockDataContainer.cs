using StocksMonitor.src.databaseWrapper;
using StocksMonitor.src.dataStoreNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

#if !SIMULATIONS
namespace StocksMonitor.src
{
    public class DataContainer { 
        private DataGridView dataGrid;
        private DataStore store;
        private int investmentTarget = 500; // TODO, make adjustable
        private int Ma200Warning = 0;
        private int overProfitWarning = 0;
        private int refillWarning = 0;

        private decimal Ma200LimitSetting;
        private decimal Ma200HighLimitSetting;
        private decimal overProfitLimitSetting;
        private decimal refillProfitLimitSetting;

        public int GetMa200Warnings { get => Ma200Warning; }
        public int GetOverOverProfitWarnings { get => overProfitWarning; }
        public int GetRefillWarnings { get => refillWarning; }

        public void SetLimits(decimal Ma200Limit, decimal Ma200HighLimit, decimal refillLimit, decimal overProfit)
        {
            Ma200LimitSetting = Ma200Limit;
            Ma200HighLimitSetting = Ma200HighLimit;
            overProfitLimitSetting = overProfit;
            refillProfitLimitSetting = refillLimit;
        }
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
                Name = "Value",
                HeaderText = "Value",
                ValueType = typeof(string)
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
        private bool MA200Warning(Stock stock)
        {
            decimal MA200 = stock.MA200;
            decimal MA200Limit = Ma200LimitSetting;
            decimal highMa200Limit = Ma200HighLimitSetting;

            if (stock.OwnedCnt > 0 && (MA200 < MA200Limit) || MA200 > highMa200Limit)
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
            decimal minValue = investmentTarget + overProfitLimitSetting / 100 * investmentTarget;
            if (valueIfSoldOne > investmentTarget &&
                (value > minValue))
            {
                return true;
            }
            return false;
        }
        private bool RefillWarning(Stock stock)
        {
            if (stock.OwnedCnt == 0)
            {
                return false;
            }

            decimal value = stock.Price * stock.OwnedCnt;
            decimal valueIfBuyOne = value + stock.Price;

            decimal minValue = investmentTarget - refillProfitLimitSetting / 100 * investmentTarget;

            if ((valueIfBuyOne < investmentTarget) &&
                (stock.MA200 > 0 && stock.MA200 < 15) &&
                (value < minValue))
            {

                return true;
            }
            return false;
        }


        public void UpdateData(bool warnings, bool hidden, bool wanted, bool intrested, bool owned)
        {
            dataGrid.Rows.Clear(); // Clear existing rows

            Ma200Warning = 0;
            overProfitWarning = 0;
            refillWarning = 0;


            foreach (var stock in store.stocks)
            {
                // Check filters, skip if
                if (
                    (warnings && !stock.filters.warning) ||
                    (hidden && stock.filters.hidden) ||
                    (wanted && stock.OwnedCnt > 0) ||
                    (intrested && !stock.filters.intrested) ||
                    (owned && stock.OwnedCnt == 0))
                {
                    continue;
                }

                // Add a new row with the stock data
                decimal earned = CalculateEarning(stock);

                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGrid,
                    stock.Name,
                    stock.Price,
                    stock.MA200.ToString("F2"),
                    stock.OwnedCnt,
                    earned,
                    stock.OwnedCnt > 0 ? stock.Price * stock.OwnedCnt : "", // Value
                    stock.filters.hidden ? "X" : "",
                    stock.filters.intrested ? "X" : "");
                dataGrid.Rows.Add(row);

                stock.filters.warning = false;

                //private string[] columns = { "Name", "Price", "MA200", "Owned", "Earned", "Value", "Hidden", "Interested"};
                if (MA200Warning(stock))
                {
                    stock.filters.warning = true;
                    Ma200Warning++;
                    row.Cells[2].Style.BackColor = Color.LightSalmon;
                }
                if (OverProfitWarning(stock))
                {
                    stock.filters.warning = true;
                    overProfitWarning++;
                    row.Cells[1].Style.BackColor = Color.LightGreen;
                }
                else if (RefillWarning(stock))
                {
                    stock.filters.warning = true;
                    refillWarning++;
                    row.Cells[1].Style.BackColor = Color.Yellow;
                }
                else if (stock.Price > investmentTarget)
                {
                    row.Cells["Price"].Style.BackColor = Color.Red;
                }
            }
            // TODO, write testcase to confirm order of columns
        }
    }
}
#endif