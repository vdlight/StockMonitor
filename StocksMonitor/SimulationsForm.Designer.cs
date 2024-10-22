namespace StocksMonitor
{
    partial class SimulationsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            closeButton = new Button();
            dataGridView1 = new DataGridView();
            intrestedButton = new Button();
            hiddenButton = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // closeButton
            // 
            closeButton.Location = new Point(12, 199);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(75, 23);
            closeButton.TabIndex = 0;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += cancelButton_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 64);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(419, 119);
            dataGridView1.TabIndex = 1;
            // 
            // intrestedButton
            // 
            intrestedButton.Location = new Point(12, 35);
            intrestedButton.Name = "intrestedButton";
            intrestedButton.Size = new Size(75, 23);
            intrestedButton.TabIndex = 2;
            intrestedButton.Text = "Intrested";
            intrestedButton.UseVisualStyleBackColor = true;
            intrestedButton.Click += intrestedButton_Click;
            // 
            // hiddenButton
            // 
            hiddenButton.Location = new Point(93, 35);
            hiddenButton.Name = "hiddenButton";
            hiddenButton.Size = new Size(75, 23);
            hiddenButton.TabIndex = 3;
            hiddenButton.Text = "Hidden";
            hiddenButton.UseVisualStyleBackColor = true;
            hiddenButton.Click += hiddenButton_Click;
            // 
            // SimulationsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1154, 779);
            Controls.Add(hiddenButton);
            Controls.Add(intrestedButton);
            Controls.Add(dataGridView1);
            Controls.Add(closeButton);
            Name = "SimulationsForm";
            Text = "Simulations";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button closeButton;
        private DataGridView dataGridView1;
        private Button intrestedButton;
        private Button hiddenButton;
        
    }
}