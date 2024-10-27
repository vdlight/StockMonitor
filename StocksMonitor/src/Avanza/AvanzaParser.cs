using StocksMonitor.Migrations;
using StocksMonitor.src.databaseWrapper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Windows.Forms;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Collections.Generic;

namespace StocksMonitor.src.avanzaParser
{
    public class AvanzaParser
    {
        public List<Stock> stocks = [];

        public List<Stock> Run(List<Stock> s)
        {
            stocks = s;

            StockMonitorLogger.WriteMsg("Parsing avanza stocks");
            var allOK = new OwnedListParser(stocks).Parse();

            foreach (var item in stocks)
            {
                // adjust decimals before writing to DB
                int decimals = 2;
                item.PurPrice = Math.Round(item.PurPrice, decimals);
                item.Price = Math.Round(item.Price, decimals);
                item.MA200 = Math.Round(item.MA200, decimals);
            }

            if (!allOK)
            {
                StockMonitorLogger.WriteMsg($"ERROR Parsing avanza stocks failed, ABORT");
            }
            else
            {
                StockMonitorLogger.WriteMsg($"Parsing avanza stocks, {stocks.Count} pcs DONE");
            }
            
            return stocks;
        }
    }
}
