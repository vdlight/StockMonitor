namespace StocksMonitor
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            consoleOutput = new RichTextBox();
            helpProvider1 = new HelpProvider();
            menuStrip1 = new MenuStrip();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            sqlCommand1 = new Microsoft.Data.SqlClient.SqlCommand();
            stockChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            timeLabel = new Label();
            timer1000 = new System.Windows.Forms.Timer(components);
            RefreshButton = new Button();
            dataGrid = new DataGridView();
            hiddenCheckBox = new CheckBox();
            intrestedCheckBox = new CheckBox();
            ownedCheckBox = new CheckBox();
            wantedCheckbox = new CheckBox();
            Ma_label = new Label();
            showWarnings_checkbox = new CheckBox();
            refillLabel = new Label();
            Ma200limit = new TextBox();
            refillTextbox = new TextBox();
            overProfit_label = new Label();
            overProfit_textbox = new TextBox();
            stockListLabel = new Label();
            clearHiddenButton = new Button();
            clearInterested = new Button();
            clearAll = new Button();
            label2 = new Label();
            hiddenButton = new Button();
            intrestedButton = new Button();
            StockFiltersGroupBox = new GroupBox();
            WarningsGroupBox = new GroupBox();
            Ma200Highlimit = new TextBox();
            oneMonthRadioButton = new RadioButton();
            oneYearRadioButton = new RadioButton();
            selectedLabel = new Label();
            fromCalander = new MonthCalendar();
            toCalender = new MonthCalendar();
            addCustomButton = new Button();
            ((System.ComponentModel.ISupportInitialize)stockChart).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGrid).BeginInit();
            StockFiltersGroupBox.SuspendLayout();
            WarningsGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // consoleOutput
            // 
            consoleOutput.Location = new Point(1, 734);
            consoleOutput.Name = "consoleOutput";
            consoleOutput.Size = new Size(821, 88);
            consoleOutput.TabIndex = 3;
            consoleOutput.Text = "";
            // 
            // menuStrip1
            // 
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(2234, 24);
            menuStrip1.TabIndex = 4;
            menuStrip1.Text = "menuStrip1";
            // 
            // sqlCommand1
            // 
            sqlCommand1.CommandTimeout = 30;
            sqlCommand1.EnableOptimizedParameterBinding = false;
            // 
            // stockChart
            // 
            stockChart.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            chartArea1.Name = "ChartArea1";
            stockChart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            stockChart.Legends.Add(legend1);
            stockChart.Location = new Point(1053, 55);
            stockChart.Name = "stockChart";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            stockChart.Series.Add(series1);
            stockChart.Size = new Size(1169, 767);
            stockChart.TabIndex = 5;
            stockChart.Text = "chart1";
            // 
            // timeLabel
            // 
            timeLabel.AutoSize = true;
            timeLabel.Location = new Point(1, 716);
            timeLabel.Name = "timeLabel";
            timeLabel.Size = new Size(81, 15);
            timeLabel.TabIndex = 6;
            timeLabel.Text = "Time: 00:00:00";
            // 
            // timer1000
            // 
            timer1000.Interval = 1000;
            timer1000.Tick += timer1000_Tick;
            // 
            // RefreshButton
            // 
            RefreshButton.Location = new Point(12, 24);
            RefreshButton.Name = "RefreshButton";
            RefreshButton.Size = new Size(57, 22);
            RefreshButton.TabIndex = 8;
            RefreshButton.Text = "Refresh";
            RefreshButton.UseVisualStyleBackColor = true;
            RefreshButton.Click += RefreshButton_Click;
            // 
            // dataGrid
            // 
            dataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGrid.Location = new Point(12, 55);
            dataGrid.Name = "dataGrid";
            dataGrid.Size = new Size(810, 646);
            dataGrid.TabIndex = 9;
            dataGrid.SelectionChanged += dataGrid_SelectionChanged;
            // 
            // hiddenCheckBox
            // 
            hiddenCheckBox.AutoSize = true;
            hiddenCheckBox.Location = new Point(315, 27);
            hiddenCheckBox.Name = "hiddenCheckBox";
            hiddenCheckBox.Size = new Size(91, 19);
            hiddenCheckBox.TabIndex = 11;
            hiddenCheckBox.Text = "Hide hidden";
            hiddenCheckBox.UseVisualStyleBackColor = true;
            // 
            // intrestedCheckBox
            // 
            intrestedCheckBox.AutoSize = true;
            intrestedCheckBox.Location = new Point(237, 27);
            intrestedCheckBox.Name = "intrestedCheckBox";
            intrestedCheckBox.Size = new Size(72, 19);
            intrestedCheckBox.TabIndex = 12;
            intrestedCheckBox.Text = "Intrested";
            intrestedCheckBox.UseVisualStyleBackColor = true;
            // 
            // ownedCheckBox
            // 
            ownedCheckBox.AutoSize = true;
            ownedCheckBox.Location = new Point(94, 27);
            ownedCheckBox.Name = "ownedCheckBox";
            ownedCheckBox.Size = new Size(64, 19);
            ownedCheckBox.TabIndex = 13;
            ownedCheckBox.Text = "Owned";
            ownedCheckBox.UseVisualStyleBackColor = true;
            // 
            // wantedCheckbox
            // 
            wantedCheckbox.AutoSize = true;
            wantedCheckbox.Location = new Point(164, 27);
            wantedCheckbox.Name = "wantedCheckbox";
            wantedCheckbox.Size = new Size(67, 19);
            wantedCheckbox.TabIndex = 15;
            wantedCheckbox.Text = "Wanted";
            wantedCheckbox.UseVisualStyleBackColor = true;
            // 
            // Ma_label
            // 
            Ma_label.AutoSize = true;
            Ma_label.Location = new Point(6, 96);
            Ma_label.Name = "Ma_label";
            Ma_label.Size = new Size(54, 15);
            Ma_label.TabIndex = 16;
            Ma_label.Text = "Ma200: 0";
            // 
            // showWarnings_checkbox
            // 
            showWarnings_checkbox.AutoSize = true;
            showWarnings_checkbox.Location = new Point(6, 60);
            showWarnings_checkbox.Name = "showWarnings_checkbox";
            showWarnings_checkbox.Size = new Size(61, 19);
            showWarnings_checkbox.TabIndex = 17;
            showWarnings_checkbox.Text = "Enable";
            showWarnings_checkbox.UseVisualStyleBackColor = true;
            // 
            // refillLabel
            // 
            refillLabel.AutoSize = true;
            refillLabel.Location = new Point(6, 123);
            refillLabel.Name = "refillLabel";
            refillLabel.Size = new Size(45, 15);
            refillLabel.TabIndex = 18;
            refillLabel.Text = "Refill: 0";
            // 
            // Ma200limit
            // 
            Ma200limit.Location = new Point(86, 85);
            Ma200limit.Name = "Ma200limit";
            Ma200limit.Size = new Size(30, 23);
            Ma200limit.TabIndex = 19;
            // 
            // refillTextbox
            // 
            refillTextbox.Location = new Point(86, 114);
            refillTextbox.Name = "refillTextbox";
            refillTextbox.Size = new Size(73, 23);
            refillTextbox.TabIndex = 20;
            // 
            // overProfit_label
            // 
            overProfit_label.AutoSize = true;
            overProfit_label.Location = new Point(6, 150);
            overProfit_label.Name = "overProfit_label";
            overProfit_label.Size = new Size(48, 15);
            overProfit_label.TabIndex = 21;
            overProfit_label.Text = "Profit: 0";
            overProfit_label.TextAlign = ContentAlignment.TopRight;
            // 
            // overProfit_textbox
            // 
            overProfit_textbox.Location = new Point(86, 146);
            overProfit_textbox.Name = "overProfit_textbox";
            overProfit_textbox.Size = new Size(73, 23);
            overProfit_textbox.TabIndex = 22;
            // 
            // stockListLabel
            // 
            stockListLabel.AutoSize = true;
            stockListLabel.Location = new Point(125, 716);
            stockListLabel.Name = "stockListLabel";
            stockListLabel.Size = new Size(106, 15);
            stockListLabel.TabIndex = 23;
            stockListLabel.Text = "Showing 0 of 0 psc";
            // 
            // clearHiddenButton
            // 
            clearHiddenButton.Location = new Point(535, 24);
            clearHiddenButton.Name = "clearHiddenButton";
            clearHiddenButton.Size = new Size(90, 22);
            clearHiddenButton.TabIndex = 25;
            clearHiddenButton.Text = "Clear Hidden";
            clearHiddenButton.UseVisualStyleBackColor = true;
            clearHiddenButton.Click += clearHiddenButton_Click;
            // 
            // clearInterested
            // 
            clearInterested.Location = new Point(631, 24);
            clearInterested.Name = "clearInterested";
            clearInterested.Size = new Size(91, 22);
            clearInterested.TabIndex = 26;
            clearInterested.Text = "Clear Intrested";
            clearInterested.UseVisualStyleBackColor = true;
            clearInterested.Click += clearInterested_Click;
            // 
            // clearAll
            // 
            clearAll.Location = new Point(728, 24);
            clearAll.Name = "clearAll";
            clearAll.Size = new Size(91, 22);
            clearAll.TabIndex = 27;
            clearAll.Text = "Clear All";
            clearAll.UseVisualStyleBackColor = true;
            clearAll.Click += clearAll_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(90, 63);
            label2.Name = "label2";
            label2.Size = new Size(66, 15);
            label2.TabIndex = 28;
            label2.Text = "Percentage";
            // 
            // hiddenButton
            // 
            hiddenButton.Location = new Point(19, 104);
            hiddenButton.Name = "hiddenButton";
            hiddenButton.Size = new Size(75, 23);
            hiddenButton.TabIndex = 30;
            hiddenButton.Text = "Hidden";
            hiddenButton.UseVisualStyleBackColor = true;
            hiddenButton.Click += hiddenButton_Click;
            // 
            // intrestedButton
            // 
            intrestedButton.Location = new Point(19, 75);
            intrestedButton.Name = "intrestedButton";
            intrestedButton.Size = new Size(75, 23);
            intrestedButton.TabIndex = 29;
            intrestedButton.Text = "Intrested";
            intrestedButton.UseVisualStyleBackColor = true;
            intrestedButton.Click += intrestedButton_Click;
            // 
            // StockFiltersGroupBox
            // 
            StockFiltersGroupBox.Controls.Add(intrestedButton);
            StockFiltersGroupBox.Controls.Add(hiddenButton);
            StockFiltersGroupBox.Location = new Point(845, 242);
            StockFiltersGroupBox.Name = "StockFiltersGroupBox";
            StockFiltersGroupBox.Size = new Size(127, 144);
            StockFiltersGroupBox.TabIndex = 32;
            StockFiltersGroupBox.TabStop = false;
            StockFiltersGroupBox.Text = "Stock filters";
            // 
            // WarningsGroupBox
            // 
            WarningsGroupBox.Controls.Add(Ma200Highlimit);
            WarningsGroupBox.Controls.Add(Ma200limit);
            WarningsGroupBox.Controls.Add(refillTextbox);
            WarningsGroupBox.Controls.Add(label2);
            WarningsGroupBox.Controls.Add(overProfit_textbox);
            WarningsGroupBox.Controls.Add(Ma_label);
            WarningsGroupBox.Controls.Add(showWarnings_checkbox);
            WarningsGroupBox.Controls.Add(refillLabel);
            WarningsGroupBox.Controls.Add(overProfit_label);
            WarningsGroupBox.Location = new Point(845, 55);
            WarningsGroupBox.Name = "WarningsGroupBox";
            WarningsGroupBox.Size = new Size(165, 181);
            WarningsGroupBox.TabIndex = 33;
            WarningsGroupBox.TabStop = false;
            WarningsGroupBox.Text = "Warnings";
            // 
            // Ma200Highlimit
            // 
            Ma200Highlimit.Location = new Point(126, 85);
            Ma200Highlimit.Name = "Ma200Highlimit";
            Ma200Highlimit.Size = new Size(30, 23);
            Ma200Highlimit.TabIndex = 29;
            // 
            // oneMonthRadioButton
            // 
            oneMonthRadioButton.AutoSize = true;
            oneMonthRadioButton.Location = new Point(1417, 30);
            oneMonthRadioButton.Name = "oneMonthRadioButton";
            oneMonthRadioButton.Size = new Size(86, 19);
            oneMonthRadioButton.TabIndex = 34;
            oneMonthRadioButton.TabStop = true;
            oneMonthRadioButton.Text = "One Month";
            oneMonthRadioButton.UseVisualStyleBackColor = true;
            oneMonthRadioButton.CheckedChanged += oneMonthRadioButton_CheckedChanged;
            // 
            // oneYearRadioButton
            // 
            oneYearRadioButton.AutoSize = true;
            oneYearRadioButton.Location = new Point(1607, 30);
            oneYearRadioButton.Name = "oneYearRadioButton";
            oneYearRadioButton.Size = new Size(72, 19);
            oneYearRadioButton.TabIndex = 35;
            oneYearRadioButton.TabStop = true;
            oneYearRadioButton.Text = "One Year";
            oneYearRadioButton.UseVisualStyleBackColor = true;
            oneYearRadioButton.CheckedChanged += oneYearRadioButton_CheckedChanged;
            // 
            // selectedLabel
            // 
            selectedLabel.AutoSize = true;
            selectedLabel.Location = new Point(268, 716);
            selectedLabel.Name = "selectedLabel";
            selectedLabel.Size = new Size(72, 15);
            selectedLabel.TabIndex = 36;
            selectedLabel.Text = "Selected psc";
            // 
            // fromCalander
            // 
            fromCalander.Location = new Point(821, 435);
            fromCalander.Name = "fromCalander";
            fromCalander.TabIndex = 37;
            // 
            // toCalender
            // 
            toCalender.Location = new Point(821, 615);
            toCalender.Name = "toCalender";
            toCalender.TabIndex = 38;
            // 
            // addCustomButton
            // 
            addCustomButton.Location = new Point(851, 411);
            addCustomButton.Name = "addCustomButton";
            addCustomButton.Size = new Size(121, 23);
            addCustomButton.TabIndex = 31;
            addCustomButton.Text = "AddCustom";
            addCustomButton.UseVisualStyleBackColor = true;
            addCustomButton.Click += addCustomButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2234, 828);
            Controls.Add(addCustomButton);
            Controls.Add(toCalender);
            Controls.Add(fromCalander);
            Controls.Add(selectedLabel);
            Controls.Add(oneYearRadioButton);
            Controls.Add(oneMonthRadioButton);
            Controls.Add(WarningsGroupBox);
            Controls.Add(StockFiltersGroupBox);
            Controls.Add(clearAll);
            Controls.Add(clearInterested);
            Controls.Add(clearHiddenButton);
            Controls.Add(stockListLabel);
            Controls.Add(wantedCheckbox);
            Controls.Add(ownedCheckBox);
            Controls.Add(intrestedCheckBox);
            Controls.Add(hiddenCheckBox);
            Controls.Add(dataGrid);
            Controls.Add(RefreshButton);
            Controls.Add(timeLabel);
            Controls.Add(stockChart);
            Controls.Add(consoleOutput);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)stockChart).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGrid).EndInit();
            StockFiltersGroupBox.ResumeLayout(false);
            WarningsGroupBox.ResumeLayout(false);
            WarningsGroupBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private RichTextBox consoleOutput;
        private HelpProvider helpProvider1;
        private MenuStrip menuStrip1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private Microsoft.Data.SqlClient.SqlCommand sqlCommand1;
        private System.Windows.Forms.DataVisualization.Charting.Chart stockChart;
        private Label timeLabel;
        private System.Windows.Forms.Timer timer1000;
        private Button RefreshButton;
        private DataGridView dataGrid;
        private CheckBox hiddenCheckBox;
        private CheckBox intrestedCheckBox;
        private CheckBox ownedCheckBox;
        private CheckBox wantedCheckbox;
        private Label Ma_label;
        private CheckBox showWarnings_checkbox;
        private Label refillLabel;
        private TextBox Ma200limit;
        private TextBox refillTextbox;
        private Label overProfit_label;
        private TextBox overProfit_textbox;
        private Label stockListLabel;
        private Button clearHiddenButton;
        private Button clearInterested;
        private Button clearAll;
        private Label label2;
        private Button hiddenButton;
        private Button intrestedButton;
        private GroupBox StockFiltersGroupBox;
        private GroupBox WarningsGroupBox;
        private RadioButton oneMonthRadioButton;
        private RadioButton oneYearRadioButton;
        private Label selectedLabel;
        private TextBox Ma200Highlimit;
        private MonthCalendar fromCalander;
        private MonthCalendar toCalender;
        private Button addCustomButton;
    }
}
