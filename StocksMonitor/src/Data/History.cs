using StocksMonitor.Data.StockNS;

namespace StocksMonitor.Data.HistoryNS
{
    public class History
    {
        public int ID { get; set; }             // ID of history

        public DateTime Date { get; set; }      // Percentage of MA200 correspond to current price
        public decimal Price { get; set; }      // current price of stock
        public decimal MA200 { get; set; }      // Percentage of MA200 correspond to current price

        public void CopyDataFromNewStock(Stock rhs)
        {
            this.MA200 = rhs.MA200;
            this.Price = rhs.Price;
        }

        // Foreign Key
        public int StockId { get; set; }        // ÍD connection as foreign key to Stock
        public Stock? Stock { get; set; }        // Navigation property
    }
}
