using System.Windows.Forms.DataVisualization.Charting;

namespace StocksMonitor.DatavisualizationNS
{
    public class DataVisualization
    {
        private Chart chart;
        
        public DataVisualization(Chart chart)
        {
            this.chart = chart;

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
    }
}