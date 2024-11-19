using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using StocksMonitor.Data.HistoryNS;
using StocksMonitor.Data.StockNS;

using StocksMonitor.LoggerNS;


namespace StocksMonitor.Storage.StockDataContextNS
{
    public class StockDataContext : DbContext
    {
        public DbSet<Stock> stockData { get; set; }
        public DbSet<History> history { get; set; }
        public DbSet<StockMisc> miscs { get; set; }

        //public const string defConnString = "Server=JENSA;Database=master;Integrated Security=True;TrustServerCertificate=True;";
        // private string connStr = "Server=JENSA;Database=master;Integrated Security=True;TrustServerCertificate=True;";


        private string connStr = "Server=NEX-5CD350FDG5;Database=Simulations;Integrated Security=True;TrustServerCertificate=True;";
        public const string defConnString = "Server=NEX-5CD350FDG5;Database=Simulations;Integrated Security=True;TrustServerCertificate=True;";

        public StockDataContext(string connString)
        {
            this.connStr = connStr;
        }

        public StockDataContext()
        {
            this.connStr = defConnString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSqlServer(connStr)
            .LogTo(StocksMonitorLogger.WriteMsg, LogLevel.Error);

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
}
