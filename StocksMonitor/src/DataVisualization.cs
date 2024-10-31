using StocksMonitor.src.databaseWrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace StocksMonitor.src
{
    public class DataVisualization
    {
        private Chart chart;
        private Random random;


        private readonly Dictionary<string, Color> stockColor = [];
        
        public DataVisualization(Chart chart)
        {
            this.chart = chart;
            random = new Random();
            // Set dark background for the chart area
            chart.ChartAreas["ChartArea1"].BackColor = Color.Gray;

            // Remove grid lines
            chart.ChartAreas["ChartArea1"].AxisX.MajorGrid.LineColor = Color.Black;
            chart.ChartAreas["ChartArea1"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.DashDot;
            chart.ChartAreas["ChartArea1"].AxisY.MajorGrid.LineColor = Color.Black;
            chart.ChartAreas["ChartArea1"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.DashDot;
            chart.ChartAreas["ChartArea1"].AxisX.MinorGrid.LineColor = Color.Black;
            chart.ChartAreas["ChartArea1"].AxisY.MinorGrid.LineColor = Color.Black;

            // Optional: Set dark background for the entire chart
            chart.BackColor = Color.Black;

            // Optional: Change axis labels and title colors to stand out against the dark background
            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.ForeColor = Color.White;
            chart.ChartAreas["ChartArea1"].AxisY.LabelStyle.ForeColor = Color.White;
            chart.ChartAreas["ChartArea1"].AxisX.TitleForeColor = Color.White;
            chart.ChartAreas["ChartArea1"].AxisY.TitleForeColor = Color.White;

        }

        private Color GetColor(string name)
        {
            if (!stockColor.ContainsKey(name) || stockColor[name].IsEmpty)
            {
                stockColor[name] = Color.FromArgb(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));
            }
            return stockColor[name];
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

            return price / (1 + (MAPercentage/ 100));
        }

        public void SelectedRows(List<String> names, List<Stock> stocks, bool rangeOneWeek)
        {
            chart.Series.Clear();

            DateTime range;

            if (rangeOneWeek)
            {
                range = DateTime.Now.AddDays(-7);
            }
            else
            {
                range = DateTime.Now.AddDays(-31);
            }

            // TODO, make it selectable what range

            for (int i = 0; i < names.Count; i++)
            {
                var stock = stocks.Single(s => s.Name == names[i]);

                if (stock != null)
                {
                    Color color = GetColor(stock.Name);
                    if (names.Count > 1) {
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
                    chart.Series[price].BorderWidth = 3;
                    chart.Series[price].MarkerStyle = MarkerStyle.Circle;
                    chart.Series[price].MarkerSize = 5;
                    chart.Series[price].ToolTip = "Date: #VALX, Price: #VALY{N2}";

                    var MA = $"MA {names[i]}";
                    chart.Series.Add(MA);
                    chart.Series[MA].Color = color;
                    chart.Series[MA].Legend = "Legend1";
                    chart.Series[MA].ChartArea = "ChartArea1";
                    chart.Series[MA].BorderDashStyle = ChartDashStyle.Dash;
                    chart.Series[MA].ChartType = SeriesChartType.Line;
                    chart.Series[MA].BorderWidth = 2;
                    chart.Series[MA].MarkerStyle = MarkerStyle.Circle;
                    chart.Series[MA].MarkerSize = 5;
                    chart.Series[MA].ToolTip = "Date: #VALX, MA200: #VALY{N2}";

                    var cnt = 0;

                    var reversedHistory = stock.History;
                    reversedHistory.Reverse();

                    foreach (var history in reversedHistory.Take(31))
                    {
                        chart.Series[price].Points.AddXY(history.Date, history.Price);
                        chart.Series[MA].Points.AddXY(history.Date, CalculateMAFromPriceAndMAPercentage(history.Price, history.MA200));
                        cnt--;
                    }
                }
                else
                {
                    StockMonitorLogger.WriteMsg($"ERROR: failed to chart stock name{names[i]}");
                }
            }
        }
    }
}
