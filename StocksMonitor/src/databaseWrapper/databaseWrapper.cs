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


#if DEBUG
        private string connString = "Server=JENSA;Database=Test;Integrated Security=True;TrustServerCertificate=True;";
#elif SIMULATIONS
        //private string connString = "Server=NEX-5CD350FDG5;Database=Simulations;Integrated Security=True;TrustServerCertificate=True;";
        private string connString = "Server=JENSA;Database=Simulations;Integrated Security=True;TrustServerCertificate=True;";
#else
        private string connString = "Server=JENSA;Database=master;Integrated Security=True;TrustServerCertificate=True;";
#endif

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSqlServer(connString)
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
        public ICollection<History> History { get; set; } = []; // One-to-Many relationship
        public StockMisc? Misc { get; set; } = new(); // One-to-One relationship

        public FilterSelections filters = new();

        public void CopyDataFromNewStock(Stock rhs) 
        {
            this.Name = rhs.Name;
            this.MA200 = rhs.MA200;
            this.Price = rhs.Price;
            this.OwnedCnt = rhs.OwnedCnt;
            this.PurPrice = rhs.PurPrice;
            this.List = rhs.List;
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
        public async Task WriteData(List<Stock> data)
        {
            StockMonitorLogger.WriteMsg("Write data to TB");
            await using (var db = new StockDataContext())
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

                    // Update History,  Check if existing date or new 

                    var existingHistory = db.history.FirstOrDefault(history => history.Date.Date == DateTime.Now.Date
                                                                && history.StockId == existingStock.ID);

                    if (existingHistory != null)
                    {
                        existingHistory.CopyDataFromNewStock(newStock);
                    }
                    else
                    {
                        db.history.Add(new()
                        {
                            MA200 = newStock.MA200,
                            Price = newStock.Price,
                            OwnedCnt = newStock.OwnedCnt,
                            Date = DateTime.Now.Date,
                            StockId = existingStock.ID
                        });
                    }
                }
                await db.SaveChangesAsync();

                StockMonitorLogger.WriteMsg($"Wrote {data.Count} stocks To DB");
            }
        }

        public async Task<List<Stock>> ReadData()
        {
            StockMonitorLogger.WriteMsg("Read data from DB");

            using (var db = new StockDataContext())
            {
                var stocksWithHistory = await db.stockData
                    .Include(S => S.History.OrderBy(h => h.Date))
                    .ToListAsync();

                StockMonitorLogger.WriteMsg($"Read {stocksWithHistory.Count} stocks from DB");
                return stocksWithHistory;

            }
        }

#if DEBUG
        public async Task UpdateHistoryDateFromTo(string fromDate, string toDate)
        {
            StockMonitorLogger.WriteMsg("Changed dates in history table");
            await using (var db = new StockDataContext())
            {
                var query =
                    "UPDATE history " +
                    "SET Date = '" + toDate +
                    "' WHERE Date = '" + fromDate + "'";

                await db.Database.ExecuteSqlRawAsync(query);
            }
        }


        public async Task ClearDatabaseTables()
        {
            StockMonitorLogger.WriteMsg("Delete all in history, stockData and miscs table");
            await using (var db = new StockDataContext())
            {
                var query =
                    "DELETE FROM history" + "\n" +
                    "DELETE FROM stockData" + "\n" +
                    "DELETE FROM miscs";

                await db.Database.ExecuteSqlRawAsync(query);
            }
        }
#endif
    }
}
