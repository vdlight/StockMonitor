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
        }
        public void UpdateData()
        {
            dataGrid.Rows.Clear(); // Clear existing rows
            // TODO, backup av simulerings / test DB, kan jag lägga upp på git, för att få ut på andra datorer. MAster bara från hemma
            // TODOD, i ett fall så kan det vara att man ska analysera eget, dvs följa owned och räkna ut avkastning
            // men i normala fall, så sker simuleringarna bara mot aktiedata, och egan actions
            // tmp
            store.stocks.Clear();

            store.stocks.Add(new Stock()
            {
                Name = "Saab",
                Price = 200,
                PurPrice = 180,
                List = "Large Cap Stockholm",
            });
            store.stocks[0].History.Add(new()
            {
                Date = DateTime.Now.Date.AddDays(-2).Date,
                MA200 = 0.6m,
                Price = 180,
            });
            store.stocks[0].History.Add(new()
            {
                Date = DateTime.Now.Date,
                MA200 = 5m,
                Price = 200,
            });

            var simu1 = new Simulation();
            simu1.Run(store.stocks);
            simu1.CalculateSimulation();
        }
    }

    public class Simulation
    {
        const decimal originalInvestment = 250m;
        List<Stock> simulatorStocks = [];
        decimal wallet = originalInvestment;

        private class Portfolio
        {
            decimal wallet;
            decimal value;
            DateTime timestamp;

            public Portfolio (DateTime date)
            {
                timestamp = date;
            }
        }
        List<Portfolio> portfolioHistory = [];

        // strategies
        public void Run(List<Stock> stocks)
        {
            simulatorStocks.AddRange(stocks);
            // TODO, find out history stamps
            // skriva owned cnt i stocklistan, per historik, för att få koll på utvekcling, väldigt lik vanliga procentuella uträkningen också
            var oldestTimestamp = DateTime.Now.AddDays(-2).Date;

            // latest hisory is the same as current, dont duplicate

            while (oldestTimestamp != DateTime.Now.AddDays(1).Date)
            {
                portfolioHistory.Add(new(oldestTimestamp));

                foreach (var stock in stocks)
                {
                    // kan skapa upp unika histrik objekt
                    // sedan lägga upp plånbok och investeringar där per historik
                    // kanske iterera från bak

                    // TODOD, få till ett val där man kan välja om man har db eller inte. För att kunna läsa upp sparade aktiedata från csv istället, där inte tillgång till db finns

                    var h = stock.History.FirstOrDefault(h => h.Date == oldestTimestamp);

                    if (h != null)
                    {
                        if(h.Price < wallet)
                        {
                            wallet-= h.Price; 
                            h.OwnedCnt++;
                        }
                    }

                }            
                oldestTimestamp = DateTime.Now.AddDays(+1).Date;
            }
            // copy latest history to current status
            foreach(var stock in simulatorStocks)
            {
                stock.OwnedCnt = stock.History.First().OwnedCnt;
            }
        }

        public decimal CalculateSimulation()
        {
            decimal value = wallet;
            foreach(var stock in simulatorStocks)
            {
                value += (stock.OwnedCnt * stock.Price);
            }

            return value;
        }
    }
}
#endif