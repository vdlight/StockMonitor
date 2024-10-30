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
        public List<Stock> Run()
        {
            StockMonitorLogger.WriteMsg("Parsing avanza stocks");
            
            var stocks = new OwnedListParser().Parse();

            if (stocks.Count == 0)
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
