using StocksMonitor.src.databaseWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksMonitor.src.avanzaParser
{
    public class OwnedListParser()
    {
        private List<string> lines = [];

        private readonly List<Stock> stocks = [];


        public OwnedListParser(List<Stock> stocks) : this()
        {
            this.stocks = stocks;
        }
        void readLines()
        {
            // read Lines
            StreamReader sr = new StreamReader("../../../src/Avanza/innehav.txt");

            var line = sr.ReadLine();

            while (line != null)
            {
                lines.Add(line);
                line = sr.ReadLine();

            }
            sr.Close();
        }

        void removeTopMostInfoText()
        {
            while (!lines[0].Contains("Land"))// Part of headline avove stocks
            {
                lines.RemoveAt(0);
            }
            lines.RemoveAt(0);


            while (lines[0] == "\t") // some empty rows
            {
                lines.RemoveAt(0);
            }
        }

        void removeBottomMostInfoText()
        {
            int startIndex = 0;

            while (!lines[startIndex].Contains("Totalt värde"))// Part of headline below stocks
            {
                startIndex++;
            }

            lines.RemoveRange(startIndex, lines.Count() - startIndex);

        }

        void populateStocks()
        {
            while (lines.Count > 0)
            {
                populateStock();
            }
        }

        void populateStock()
        {
            string name;
            decimal purPrice;
            int count;

            name = lines[0];
            lines.RemoveAt(0);

            var data = lines[0].Split("\t");     // data values, are seperated by tabs
            lines.RemoveAt(0);

            count = int.Parse(data[0]);  // count is next, simple int

            purPrice = decimal.Parse(data[1].Substring(0, data[1].Length - 3).Trim());   // last is pur price, remove SEK in the end

            decimal latestPrice = decimal.Parse(data[2]);
            while (lines.Count > 0 && lines[0].Contains("\t"))
            {
                lines.RemoveAt(0); // remove blank rows
            }

            foreach (var stock in stocks)
            {
                // TODO, checking here for name, but also for latest price, for feasability check betwen bd och avanza
                if (stock.Name == name)
                {
                    if (latestPrice == stock.Price)
                    {
                        stock.OwnedCnt = count;
                        stock.PurPrice = purPrice;
                    }
                    else
                    {
                        StockMonitorLogger.WriteMsg("ERRror, prices does not match for " + stock.Name + " " + latestPrice
                            + " " + stock.Price);
                    }
                }
            }
        }

        private bool CorrectPageParsed()
        {
            var expectedStack = new Stack<string>(
            [
                "Nyckeltal",
                "Historik",
                "Inköpsinfo",
                "Utveckling",
                "Min konfig",
                "Aktier"
            ]);

            var expectedHeadlineCategories = " Land\tNamn\tAntal\tInköpskurs\tSenast\tVerktyg";

            
            foreach (var row in lines)
            {
                if (expectedStack.Count > 0 && row == expectedStack.Peek())
                {
                    expectedStack.Pop();
                }
                if (row == expectedHeadlineCategories)
                {
                    expectedHeadlineCategories = "";
                }
            }
            expectedHeadlineCategories = "";
            // TODO, skipping headline check just now
            if (expectedStack.Count != 0 || expectedHeadlineCategories != "")
            {
                StockMonitorLogger.WriteMsg("ERROR: When parsing innehav, headlines does not correspond expected, ABORT");
                return false;
            }

            return true;
        }


        public bool Parse()
        {
            StockMonitorLogger.WriteMsg("Parsing owned Avanza stocks");
            readLines();
            if (!CorrectPageParsed())
            {
                return false;
            }

            removeTopMostInfoText();
            removeBottomMostInfoText();

            populateStocks();

            return true;
        }
    }
}
