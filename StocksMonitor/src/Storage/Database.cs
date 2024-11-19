using Microsoft.EntityFrameworkCore;

using StocksMonitor.LoggerNS;
using StocksMonitor.Storage.StockDataContextNS;
using StocksMonitor.Data.StockNS;  

namespace StocksMonitor.Storage.DatabaseNS
{
    public class Database
    {
        public async Task WriteData(IReadOnlyList<Stock> data, string connStr = StockDataContext.defConnString)
        {
            StocksMonitorLogger.WriteMsg("Write data to DB");
            await using (var db = new StockDataContext(connStr))
            {
                foreach (var newStock in data)
                {
                    // Save data. Check if new stock or existing one in DB
                    var existingStock = db.stockData.FirstOrDefault(stock => stock.Name == newStock.Name);

                    if (existingStock != null)
                    {
                        existingStock.CopyDataFromNewStock(newStock);
                    }
                    else
                    {
                        db.stockData.Add(newStock);
                        existingStock = newStock;
                    }
                    await db.SaveChangesAsync();
                }
                StocksMonitorLogger.WriteMsg($"Wrote {data.Count} stocks To DB");
            }
        }

        public async Task<List<Stock>> ReadData(string connStr = StockDataContext.defConnString)
        {
            StocksMonitorLogger.WriteMsg("Read data from DB");

            using (var db = new StockDataContext(connStr))
            {
                var stocksWithHistory = await db.stockData
                    .Include(S => S.History.OrderBy(h => h.Date))
                    .ToListAsync();

                StocksMonitorLogger.WriteMsg($"Read {stocksWithHistory.Count} stocks from DB");
                return stocksWithHistory;

            }
        }
    }
}
