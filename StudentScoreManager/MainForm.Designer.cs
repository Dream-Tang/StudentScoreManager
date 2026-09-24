namespace StudentScoreManager
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
            btnImportExcel = new Button();
            btnDownloadTemplate = new Button();
            chkOverwrite = new CheckBox();
            button1 = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            tabControl1.SuspendLayout();
            SuspendLayout();
            // 
            // btnImportExcel
            // 
            btnImportExcel.Location = new Point(21, 146);
            btnImportExcel.Name = "btnImportExcel";
            btnImportExcel.Size = new Size(245, 91);
            btnImportExcel.TabIndex = 0;
            btnImportExcel.Text = "导入excel数据";
            btnImportExcel.UseVisualStyleBackColor = true;
            btnImportExcel.Click += btnImportExcel_Click;
            // 
            // btnDownloadTemplate
            // 
            btnDownloadTemplate.Location = new Point(21, 32);
            btnDownloadTemplate.Name = "btnDownloadTemplate";
            btnDownloadTemplate.Size = new Size(245, 91);
            btnDownloadTemplate.TabIndex = 1;
            btnDownloadTemplate.Text = "下载导入模板";
            btnDownloadTemplate.UseVisualStyleBackColor = true;
            btnDownloadTemplate.Click += btnDownloadTemplate_Click;
            // 
            // chkOverwrite
            // 
            chkOverwrite.AutoSize = true;
            chkOverwrite.Checked = true;
            chkOverwrite.CheckState = CheckState.Checked;
            chkOverwrite.Location = new Point(292, 178);
            chkOverwrite.Name = "chkOverwrite";
            chkOverwrite.Size = new Size(108, 28);
            chkOverwrite.TabIndex = 2;
            chkOverwrite.Text = "覆盖导入";
            chkOverwrite.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new Point(21, 253);
            button1.Name = "button1";
            button1.Size = new Size(245, 91);
            button1.TabIndex = 3;
            button1.Text = "导入excel数据";
            button1.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(425, 56);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(346, 354);
            tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            tabPage1.Location = new Point(4, 33);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(338, 317);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 33);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(292, 113);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tabControl1);
            Controls.Add(button1);
            Controls.Add(chkOverwrite);
            Controls.Add(btnDownloadTemplate);
            Controls.Add(btnImportExcel);
            Name = "MainForm";
            Text = "Form1";
            Load += MainForm_Load;
            tabControl1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnImportExcel;
        private Button btnDownloadTemplate;
        private CheckBox chkOverwrite;
        private Button button1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
    }
}
