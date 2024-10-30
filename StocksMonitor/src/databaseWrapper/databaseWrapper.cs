using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using System.Collections;

namespace StocksMonitor.src.databaseWrapper
{
    public class StockDataContext : DbContext
    {
        public DbSet<Stock> stockData { get; set; }
        public DbSet<History> history { get; set; }
        public DbSet<StockMisc> miscs { get; set; }

        public const string defConnString = "Server=JENSA;Database=master;Integrated Security=True;TrustServerCertificate=True;";
        //public const string defConnString = "Server=NEX-5CD350FDG5;Database=Simulations;Integrated Security=True;TrustServerCertificate=True;";
        private string connStr = defConnString;


        public StockDataContext(string connString)
        {
            this.connStr = connStr;
        }
        

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSqlServer(connStr)
            .LogTo(StockMonitorLogger.WriteMsg, LogLevel.Error);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the Stock entity
            modelBuilder.Entity<Stock>()
               .HasKey(s => s.ID); // Primary key

            modelBuilder.Entity<Stock>()
                .Property(s => s.Name)
                .HasMaxLength(130)
                .IsRequired(); // Setting as NOT NULL

            modelBuilder.Entity<Stock>()
                .Property(s => s.MA200)
                .HasColumnType("decimal(10,2)")
                .IsRequired(); // Setting as NOT NULL

            modelBuilder.Entity<Stock>()
                .Property(s => s.Price)
                .HasColumnType("decimal(10,2)")
                .IsRequired(); // Setting as NOT NULL

            modelBuilder.Entity<Stock>()
                 .Property(s => s.OwnedCnt)
                 .IsRequired(); // Setting as NOT NULL                 // INT is default ok mapping, no special handling needed

            modelBuilder.Entity<Stock>()
                .Property(s => s.PurPrice)
                .HasColumnType("decimal(10,2)")
                .IsRequired(); // Setting as NOT NULL

            modelBuilder.Entity<Stock>()
                .Property(s => s.List)
                .HasMaxLength(130)
                .IsRequired();

            modelBuilder.Entity<Stock>()
                .Property(s => s.Divident)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            modelBuilder.Entity<Stock>()
                .Property(s => s.PeValue)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            // HISTORY
            modelBuilder.Entity<History>()
                .HasKey(h => h.ID); // Set ID as primary key

            modelBuilder.Entity<History>()
                .Property(h => h.Date)
                .IsRequired();  // Setting as NOT NULL

            modelBuilder.Entity<History>()
                .Property(h => h.Price)
                .HasColumnType("decimal(10,2)")
                .IsRequired(); // Setting as NOT NULL

            modelBuilder.Entity<History>()
                .Property(h => h.MA200)
                .HasColumnType("decimal(10,2)")
                .IsRequired(); // Setting as NOT NULL

            modelBuilder.Entity<History>()
                .Property(h => h.Price)
                .HasColumnType("decimal(10,2)")
                .IsRequired(); // Setting as NOT NULL

            modelBuilder.Entity<Stock>()
                .Property(s => s.OwnedCnt)
                .IsRequired(); // Setting as NOT NULL                 // INT is default ok mapping, no special handling needed

            // Set up foreign key relationship
            modelBuilder.Entity<History>()
                .HasOne(h => h.Stock) // Each History has one Stock
                .WithMany(s => s.History) // Each Stock can have many Histories
                .HasForeignKey(h => h.StockId); // Foreign key is StockId


            // MISC
            modelBuilder.Entity<StockMisc>()
                .HasKey(m => m.ID);     // Set ID as primary key

            modelBuilder.Entity<StockMisc>()
                .Property(m => m.IsHidden)
                .HasColumnType("BIT")// max length of description text
                .IsRequired();

            modelBuilder.Entity<StockMisc>()
                .Property(m => m.HiddenReason)
                .HasMaxLength(200);      // max length of description text

            modelBuilder.Entity<StockMisc>()
                 .HasOne(m => m.Stock)   // Each StockMisc has one Stock
                 .WithOne(s => s.Misc)   // Each Stock has one StockMisc
                 .HasForeignKey<StockMisc>(m => m.StockId)
                 .IsRequired(); // Foreign key property in StockMisc
        }
    }


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

    public class History
    {
        public int ID { get; set; }             // ID of history

        public DateTime Date { get; set; }      // Percentage of MA200 correspond to current price
        public decimal Price { get; set; }      // current price of stock
        public decimal MA200 { get; set; }      // Percentage of MA200 correspond to current price
        public int OwnedCnt { get; set; }       // Number of stocks i own, 0 --> Not owned at all

        public void CopyDataFromNewStock(Stock rhs)
        {
            this.MA200 = rhs.MA200;
            this.Price = rhs.Price;
            this.OwnedCnt = rhs.OwnedCnt;
        }

        // Foreign Key
        public int StockId { get; set; }        // ÍD connection as foreign key to Stock
        public Stock? Stock { get; set; }        // Navigation property
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


    public class Storage
    {
        public async Task WriteData(IReadOnlyList<Stock> data, string connStr = StockDataContext.defConnString)
        {
            StockMonitorLogger.WriteMsg("Write data to DB");
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
                StockMonitorLogger.WriteMsg($"Wrote {data.Count} stocks To DB");
            }
        }

        public async Task<List<Stock>> ReadData(string connStr = StockDataContext.defConnString)
        {
            StockMonitorLogger.WriteMsg("Read data from DB");

            using (var db = new StockDataContext(connStr))
            {
                var stocksWithHistory = await db.stockData
                    .Include(S => S.History.OrderBy(h => h.Date))
                    .ToListAsync();

                StockMonitorLogger.WriteMsg($"Read {stocksWithHistory.Count} stocks from DB");
                return stocksWithHistory;

            }
        }
    }
}
