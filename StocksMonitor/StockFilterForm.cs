using StocksMonitor.src.databaseWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StocksMonitor
{
    public partial class StockFilterForm : Form
    {
        private string[] columns = { "Name", "Interested", "Hidden", "Price", "MA200"};
        public Stock stock;

        public StockFilterForm(Stock stock)
        {
            this.stock = stock;
            InitializeComponent();


            dataGridView1.Columns.Clear(); // Clear any existing columns

            // Define your columns
            dataGridView1.Columns.Add(columns[0], columns[0]);
            dataGridView1.Columns.Add(columns[1], columns[1]);
            dataGridView1.Columns.Add(columns[2], columns[2]);
            dataGridView1.Columns.Add(columns[3], columns[3]);
            dataGridView1.Columns.Add(columns[4], columns[4]);

            // Set properties for better appearance
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                if (i == 0)
                {
                    // name shall be leftaligned, rest, center
                    dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    continue;
                }
                dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            }
                
            decimal ma200Value, earned;
            if (stock.MA200 > 0)
            {
                ma200Value = stock.Price - (stock.MA200 / 100 * stock.Price);
            }
            else
            {
                ma200Value = stock.Price + (stock.MA200 / 100 * stock.Price);
            }
            if (stock.OwnedCnt > 0)
            {
                earned = ((stock.Price - stock.PurPrice) / stock.PurPrice) * 100;
            }
            else
            {
                earned = 0;
            }

            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridView1,
            stock.Name,
                stock.Price.ToString("F1"),
                ma200Value.ToString("F1"),
                stock.OwnedCnt.ToString(),
                earned.ToString("F1") + "%");

            dataGridView1.Rows.Add(row);

            updateButtonColors();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void intrestedButton_Click(object sender, EventArgs e)
        {
            stock.filters.intrested = !stock.filters.intrested;

            if (stock.filters.intrested)
            {
                stock.filters.hidden = false;
            }

            updateButtonColors();
        }

        private void updateButtonColors()
        {
            var selectedColor = Color.Green;
            var defaultColor = SystemColors.Control;

            intrestedButton.BackColor = stock.filters.intrested ? selectedColor : defaultColor;
            hiddenButton.BackColor = stock.filters.hidden ? selectedColor : defaultColor;
        }

        private void hiddenButton_Click(object sender, EventArgs e)
        {
            stock.filters.hidden = !stock.filters.hidden;

            if (stock.filters.hidden)
            {
                stock.filters.intrested = false;
            }

            updateButtonColors();
        }
    }
}
