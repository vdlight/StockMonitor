using StocksMonitor.Data.HistoryNS;

namespace StocksMonitor.Data.StockNS
{
    public class Stock
    {
        public int ID { get; set; }             // ID of stock --> PRIMARY KEY
        public string? Name { get; set; }        // Name of stock
        public decimal MA200 { get; set; }      // Percentage of MA200 correspond to current price
        public decimal Price { get; set; }      // current price of stock
        public int OwnedCnt { get; set; }       // Number of stocks i own, 0 --> Not owned at all
        public decimal PurPrice { get; set; }   // My purchase price per stock at investment
        public string List { get; set; }       // List stock belongs to
        public decimal Divident { get; set; }
        public decimal PeValue { get; set; }
        public List<History> History { get; set; } = []; // One-to-Many relationship
        public StockMisc? Misc { get; set; } = new(); // One-to-One relationship

        public bool IsIndex = false;

        public FilterSelections filters = new();

        public void CopyDataFromNewStock(Stock rhs)
        {
            this.Name = rhs.Name;
            this.MA200 = rhs.MA200;
            this.Price = rhs.Price;
            this.OwnedCnt = rhs.OwnedCnt;
            this.PurPrice = rhs.PurPrice;
            this.Divident = rhs.Divident;
            this.PeValue = rhs.PeValue;
            this.List = rhs.List;
            this.History = rhs.History;
        }

        public class FilterSelections
        {
            public bool intrested = false;
            public bool hidden = false;
            public bool warning = false;
        }
    }

    public class StockMisc
    {
        public int ID { get; set; }                 // Primary key
        public bool IsHidden { get; set; }          // If stock is to be hidden

        public string? HiddenReason { get; set; }    // Description on why the stock is not in the list

        // Foreign key
        public int StockId { get; set; }        // ÍD connection as foreign key to Stock
        public Stock? Stock { get; set; }        // Navigation property
    }
}
