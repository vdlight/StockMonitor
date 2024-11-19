﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StocksMonitor.Storage.StockDataContextNS;

#nullable disable

namespace StocksMonitor.Migrations
{
    [DbContext(typeof(StockDataContext))]
    [Migration("20240925091736_AddOwnedCountToHistory")]
    partial class AddOwnedCountToHistory
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("StocksMonitor.src.databaseWrapper.History", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("MA200")
                        .HasColumnType("decimal(10,2)");

                    b.Property<decimal>("OwnedCnt")
                        .HasColumnType("decimal(10,2)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(10,2)");

                    b.Property<int>("StockId")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("StockId");

                    b.ToTable("history");
                });

            modelBuilder.Entity("StocksMonitor.src.databaseWrapper.Stock", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<decimal>("MA200")
                        .HasColumnType("decimal(10,2)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(130)
                        .HasColumnType("nvarchar(130)");

                    b.Property<int>("OwnedCnt")
                        .HasColumnType("int");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(10,2)");

                    b.Property<decimal>("PurPrice")
                        .HasColumnType("decimal(10,2)");

                    b.HasKey("ID");

                    b.ToTable("stockData");
                });

            modelBuilder.Entity("StocksMonitor.src.databaseWrapper.StockMisc", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("HiddenReason")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("BIT");

                    b.Property<int>("StockId")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("StockId")
                        .IsUnique();

                    b.ToTable("miscs");
                });

            modelBuilder.Entity("StocksMonitor.src.databaseWrapper.History", b =>
                {
                    b.HasOne("StocksMonitor.src.databaseWrapper.Stock", "Stock")
                        .WithMany("History")
                        .HasForeignKey("StockId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("StocksMonitor.src.databaseWrapper.StockMisc", b =>
                {
                    b.HasOne("StocksMonitor.src.databaseWrapper.Stock", "Stock")
                        .WithOne("Misc")
                        .HasForeignKey("StocksMonitor.src.databaseWrapper.StockMisc", "StockId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("StocksMonitor.src.databaseWrapper.Stock", b =>
                {
                    b.Navigation("History");

                    b.Navigation("Misc");
                });
#pragma warning restore 612, 618
        }
    }
}
