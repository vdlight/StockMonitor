using StocksMonitor.src.databaseWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksMonitor.src.avanzaParser
{
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
                else
                {
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
                var divident = splitted[3];
                var pe = splitted[4];

                bool negativeSign;

                if (sma != "-") // if SMA has not been calculated, to new stock
                {
                    negativeSign = sma[0] == '-';
                    if (negativeSign)
                    {
                        sma = sma.Substring(1); // remove negative sign
                    }

                    stock.MA200 = negativeSign ? -1 * decimal.Parse(sma) : decimal.Parse(sma);
                }

                negativeSign = pe[0] == '-';
                if (negativeSign)
                {
                    pe = pe.Substring(1); // remove negative sign
                }


                if (price != "-")
                {
                    stock.Price = decimal.Parse(splitted[1]);
                }
                stock.List = splitted[2];
                stock.Divident = decimal.Parse(divident);
                stock.PeValue = negativeSign ? -1 * decimal.Parse(pe) : decimal.Parse(pe);
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

            var expectedHeadlineCategories = "Avs. SMA 200 %\tSenast\tLista\tDirektavk. %\tP/E-tal";


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
                StockMonitorLogger.WriteMsg("ERROR: When parsing StockList, headlines does not correspond expected, ABORT");
                return false;
            }

            return true;
        }

        public bool Parse()
        {
            StockMonitorLogger.WriteMsg("Parsing Avanza stock list");
            Read();
            if (!CorrectPageParsed())
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
}
