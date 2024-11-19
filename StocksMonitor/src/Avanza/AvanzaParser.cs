using StocksMonitor.LoggerNS;
using StocksMonitor.Data.StockNS;
using StocksMonitor.Avanza.OwnedListParserNS;

namespace StocksMonitor.Avanza.AvanzaParserNS
{
    public class AvanzaParser
    {
        public List<Stock> Run()
        {
            StocksMonitorLogger.WriteMsg("Parsing avanza stocks");
            
            var stocks = new OwnedListParser().Parse();

            if (stocks.Count == 0)
            {
                StocksMonitorLogger.WriteMsg($"ERROR Parsing avanza stocks failed, ABORT");
            }
            else
            {
                StocksMonitorLogger.WriteMsg($"Parsing avanza stocks, {stocks.Count} pcs DONE");
            }
            
            return stocks;
        }
    }
}
