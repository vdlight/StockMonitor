using StocksMonitor.src.databaseWrapper;
using StocksMonitor.src.dataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
                Name = "Value ",
                HeaderText = "Value ",
                ValueType = typeof(decimal)
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Development %",
                HeaderText = "Development %",
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
            dataGrid.Rows.Clear(); // Clear existing rows


           
        }
    }
}
#endif