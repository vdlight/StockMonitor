using System.Windows.Forms.DataVisualization.Charting;

using StocksMonitor.DatavisualizationNS;
using StocksMonitor.LoggerNS;
using StocksMonitor.Data.HistoryNS;
using StocksMonitor.Data.StockNS;

namespace StocksMonitor.Simulation.VisualizationNS
{
    public class SimulationDataVisualization : DataVisualization 
    {
        private Chart chart;
        private Random random;

        private readonly Dictionary<string, Color> stockColor = [];

        public SimulationDataVisualization(Chart chart) 
            : base(chart)
        {
            this.chart = chart;
            random = new Random();

        }
        private decimal CalculateMAFromPriceAndMAPercentage(decimal price, decimal MAPercentage)
        {

            /*          If the price is 100 and MA200 is -2 (indicating the price is 2% below its 200-day moving average), the calculation will be:
             *          100/(1+(−2/100))=100/0.98=102.04
             *          This adjusts the price relative to the moving average level.
             *          If MA200 is 5 (price is 5% above the MA), then:
             *          100/(1+(5/100))=100/1.05=95.24
             *          This adjusts the price down to match the MA level.
             */

            return price / (1 + (MAPercentage / 100));
        }

        private Color GetColor(string name)
        {
            if (!stockColor.ContainsKey(name) || stockColor[name].IsEmpty)
            {
                stockColor[name] = Color.FromArgb(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));
            }
            return stockColor[name];
        }

        public void SelectedRows(List<String> names, List<Stock> stocks, bool rangeOneYear)
        {
            chart.Series.Clear();

            for (int i = 0; i < names.Count; i++)
            {
                var stock = stocks.Single(s => s.Name == names[i]);

                if (stock != null)
                {
                    Color color = GetColor(stock.Name);
                    if (names.Count > 1)
                    {
                        color = GetColor(stock.Name);
                    }
                    else
                    {
                        color = Color.DarkBlue;

                    }
                    // TODO, markera flera aktier för jämförelse, ritar inte graf korrekt längre.

                    var price = $"Price {names[i]}";
                    chart.Series.Add(price);
                    chart.Series[price].Color = color;
                    chart.Series[price].Legend = "Legend1";
                    chart.Series[price].ChartArea = "ChartArea1";
                    chart.Series[price].ChartType = SeriesChartType.Line;
                    chart.Series[price].BorderWidth = 2;
                    chart.Series[price].MarkerStyle = MarkerStyle.Circle;
                    chart.Series[price].MarkerSize = 2;
                    chart.Series[price].ToolTip = "Date: #VALX, Price: #VALY{N2}";

                    var MA = $"MA {names[i]}";
                    chart.Series.Add(MA);
                    chart.Series[MA].Color = color;
                    chart.Series[MA].Legend = "Legend1";
                    chart.Series[MA].ChartArea = "ChartArea1";
                    chart.Series[MA].BorderDashStyle = ChartDashStyle.DashDot;
                    chart.Series[MA].ChartType = SeriesChartType.Line;
                    chart.Series[MA].BorderWidth = 1;
                    chart.Series[MA].MarkerStyle = MarkerStyle.Circle;
                    chart.Series[MA].MarkerSize = 1;
                    chart.Series[MA].ToolTip = "Date: #VALX, MA200: #VALY{N2}";

                    IEnumerable<History> historyScope;
                    
                    if(rangeOneYear)
                    {
                        historyScope = stock.History.Where(h => h.Date > stock.History.Last().Date.AddYears(-1));
                    } 
                    else
                    {
                        historyScope = stock.History.Where(h => h.Date > stock.History.Last().Date.AddMonths(-1));
                    }

                    foreach (var history in historyScope)
                    {
                        chart.Series[price].Points.AddXY(history.Date, history.Price);
                        chart.Series[MA].Points.AddXY(history.Date, CalculateMAFromPriceAndMAPercentage(history.Price, history.MA200));
                    }
                }
                else
                {
                    StocksMonitorLogger.WriteMsg($"ERROR: failed to chart stock name{names[i]}");
                }
            }
        }
    }
}