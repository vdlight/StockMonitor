using StocksMonitor.Migrations;
using StocksMonitor.src.databaseWrapper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Windows.Forms;
using System;

namespace StocksMonitor.src.avanzaParser
{
    public class AvanzaParser
    {
        public List<Stock> stocks = [];
        private const decimal minimumPrice = 5;

        public List<Stock> Run()
        {

            StockMonitorLogger.WriteMsg("Parsing avanza stocks");
            var allOK =  new StockListParser(stocks).Parse();
                allOK &= new OwnedListParser(stocks).Parse();


            var skipCount = stocks.Count(s => s.Price < minimumPrice);

            StockMonitorLogger.WriteMsg($"Skipping {skipCount} stocks that are below, {minimumPrice} kr");

            stocks.RemoveAll(s => s.Price < minimumPrice);

            foreach (var item in stocks)
            {
                // adjust decimals before writing to DB
                int decimals = 2;
                item.PurPrice = Math.Round(item.PurPrice, decimals);
                item.Price = Math.Round(item.Price, decimals);
                item.MA200 = Math.Round(item.MA200, decimals);
            }

            if (!allOK)
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

    public class StockListParser
    {
        private List<string> lines = new List<string>();

        int i = 0;
        private readonly List<Stock> stocks;

        public StockListParser(List<Stock> stocks)
        {
            this.stocks = stocks;
        }

        void Read()
        {
            // read Lines
            StreamReader sr = new StreamReader("../../../src/avanzaParser/SE_Listan.txt");

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
            while (!lines[0].Contains("Köp"))// Part of headline above stocks
            {
                lines.RemoveAt(0);
            }
            lines.RemoveAt(0);


            while (lines[0] == "" || lines[0] == "\t") // some empty rows
            {
                lines.RemoveAt(0);
            }
        }
        void removeBottomMostInfoText()
        {
            int startIndex = 0;

            while (!lines[startIndex].Contains("OBS! Om du inte är inloggad på sajten"))// Part of headline below stocks
            {
                startIndex++;
            }

            lines.RemoveRange(startIndex, lines.Count() - startIndex);
        }
        void populateStockNames()
        {
            while (!lines[0].Contains("Avs. SMA"))
            {
              
                if (lines[0] == "" || lines[0] == "\t" || lines[0] == "Köp")
                {
                    lines.RemoveAt(0);
                }
                else {
                    populateStockName();
                }
            }
        }

        void populateStockName()
        {
            string name;
            name = lines[0];
            lines.RemoveAt(0);

            stocks.Add(new Stock
            {
                Name = name,
            });

        }
        void populateStockSMAValues()
        {
            while (lines[0].Contains("Avs. SMA"))
            {
                lines.RemoveAt(0);
            }

            foreach (var stock in stocks)
            {
                var line = lines[0];
                lines.RemoveAt(0);
                i++;

                var splitted = line.Split('\t');
                var sma = splitted[0];
                var price = splitted[1];
                var list = splitted[2];

                if (sma != "-") // if SMA has not been calculated, to new stock
                {
                    var negativeSign = sma[0] == '-';
                    if (negativeSign)
                    {
                        sma = sma.Substring(1); // remove negative sign
                    }

                    stock.MA200 = negativeSign ? -1 * decimal.Parse(sma) : decimal.Parse(sma);
                }

                if(price != "-") 
                { 
                    stock.Price = decimal.Parse(splitted[1]);
                }

                stock.List = list;
            }
        }

        private bool CorrectPageParsed()
        {
            var expectedStack = new Stack<string>(
            [
                "Anpassad",
                "Jensa Monitor",
                "Historik",
                "Kurs",
                "Standard",
            ]);

            var expectedHeadlineCategories = "Avs. SMA 200 %\tSenast\tLista";

            foreach (var row in lines)
            {
                if(expectedStack.Count > 0 && row == expectedStack.Peek())
                {
                    expectedStack.Pop();    
                }
                if(row == expectedHeadlineCategories)
                {
                    expectedHeadlineCategories = "";
                }
            }
            if(expectedStack.Count != 0 || expectedHeadlineCategories != "")
            {
                StockMonitorLogger.WriteMsg("ERROR: When parsing StockList, headlines does not correspond expected, ABORT");
                return false;
            }

            return true;
        }

        public bool Parse()
        {
            StockMonitorLogger.WriteMsg("Parsing Avanza stock list");
            Read();
            if(! CorrectPageParsed())
            {
                return false;
            }
            removeTopMostInfoText();
            removeBottomMostInfoText();

            populateStockNames();
            populateStockSMAValues();

            return true;
        }
    }

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
            StreamReader sr = new StreamReader("../../../src/avanzaParser/innehav.txt");

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

            purPrice = decimal.Parse(data[4].Substring(0, data[4].Length - 3).Trim());   // last is pur price, remove SEK in the end

            while (lines.Count > 0 && lines[0].Contains("\t"))
            {
                lines.RemoveAt(0); // remove blank rows
            }


            foreach( var stock in stocks)
            {
                if(stock.Name == name)
                {
                    stock.OwnedCnt = count;
                    stock.PurPrice = purPrice;
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
            
            var expectedHeadlineCategories = " Land\tNamn \t Antal\t 1 vecka\t 1 dag %\t Värde\t Inköpskurs\t Direktavkastn.\t Sedan köp %\tVerktyg";


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
            if(! CorrectPageParsed() ){
                return false;
            }

            removeTopMostInfoText();
            removeBottomMostInfoText();

            populateStocks();

            return true;
        }
    }
}
