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
            BtnConfirmExport = new Button();
            tabControl1 = new TabControl();
            学生导出设置 = new TabPage();
            label4 = new Label();
            label3 = new Label();
            txtStudentKeyword = new TextBox();
            cmbClass = new ComboBox();
            tabPage2 = new TabPage();
            label6 = new Label();
            label5 = new Label();
            label2 = new Label();
            label1 = new Label();
            txtCourseKeyword = new TextBox();
            cmbLogClass = new ComboBox();
            dpEndDate = new DateTimePicker();
            dpStartDate = new DateTimePicker();
            btnStartUse = new Button();
            tabControl1.SuspendLayout();
            学生导出设置.SuspendLayout();
            tabPage2.SuspendLayout();
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
            // BtnConfirmExport
            // 
            BtnConfirmExport.Location = new Point(437, 375);
            BtnConfirmExport.Name = "BtnConfirmExport";
            BtnConfirmExport.Size = new Size(301, 89);
            BtnConfirmExport.TabIndex = 3;
            BtnConfirmExport.Text = "确认导出";
            BtnConfirmExport.UseVisualStyleBackColor = true;
            BtnConfirmExport.Click += BtnConfirmExport_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(学生导出设置);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(429, 32);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(600, 326);
            tabControl1.TabIndex = 4;
            // 
            // 学生导出设置
            // 
            学生导出设置.Controls.Add(label4);
            学生导出设置.Controls.Add(label3);
            学生导出设置.Controls.Add(txtStudentKeyword);
            学生导出设置.Controls.Add(cmbClass);
            学生导出设置.Location = new Point(4, 33);
            学生导出设置.Name = "学生导出设置";
            学生导出设置.Padding = new Padding(3);
            学生导出设置.Size = new Size(592, 289);
            学生导出设置.TabIndex = 0;
            学生导出设置.Text = "学生导出设置";
            学生导出设置.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(183, 99);
            label4.Name = "label4";
            label4.Size = new Size(126, 24);
            label4.TabIndex = 3;
            label4.Text = "搜索姓名/学号";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(227, 49);
            label3.Name = "label3";
            label3.Size = new Size(82, 24);
            label3.TabIndex = 2;
            label3.Text = "选择班级";
            // 
            // txtStudentKeyword
            // 
            txtStudentKeyword.Location = new Point(326, 93);
            txtStudentKeyword.Name = "txtStudentKeyword";
            txtStudentKeyword.Size = new Size(237, 30);
            txtStudentKeyword.TabIndex = 1;
            // 
            // cmbClass
            // 
            cmbClass.FormattingEnabled = true;
            cmbClass.Items.AddRange(new object[] { "全部" });
            cmbClass.Location = new Point(326, 44);
            cmbClass.Name = "cmbClass";
            cmbClass.Size = new Size(237, 32);
            cmbClass.TabIndex = 0;
            cmbClass.Text = "全部";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(label6);
            tabPage2.Controls.Add(label5);
            tabPage2.Controls.Add(label2);
            tabPage2.Controls.Add(label1);
            tabPage2.Controls.Add(txtCourseKeyword);
            tabPage2.Controls.Add(cmbLogClass);
            tabPage2.Controls.Add(dpEndDate);
            tabPage2.Controls.Add(dpStartDate);
            tabPage2.Location = new Point(4, 33);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(592, 289);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "日志导出设置";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(223, 188);
            label6.Name = "label6";
            label6.Size = new Size(136, 24);
            label6.TabIndex = 7;
            label6.Text = "课程名称关键词";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(277, 143);
            label5.Name = "label5";
            label5.Size = new Size(82, 24);
            label5.TabIndex = 6;
            label5.Text = "选择班级";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(223, 76);
            label2.Name = "label2";
            label2.Size = new Size(82, 24);
            label2.TabIndex = 5;
            label2.Text = "结束时间";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(223, 21);
            label1.Name = "label1";
            label1.Size = new Size(82, 24);
            label1.TabIndex = 4;
            label1.Text = "开始时间";
            // 
            // txtCourseKeyword
            // 
            txtCourseKeyword.Location = new Point(388, 182);
            txtCourseKeyword.Name = "txtCourseKeyword";
            txtCourseKeyword.Size = new Size(165, 30);
            txtCourseKeyword.TabIndex = 3;
            // 
            // cmbLogClass
            // 
            cmbLogClass.FormattingEnabled = true;
            cmbLogClass.Items.AddRange(new object[] { "全部" });
            cmbLogClass.Location = new Point(388, 140);
            cmbLogClass.Name = "cmbLogClass";
            cmbLogClass.Size = new Size(165, 32);
            cmbLogClass.TabIndex = 2;
            cmbLogClass.Text = "全部";
            // 
            // dpEndDate
            // 
            dpEndDate.Location = new Point(307, 71);
            dpEndDate.Name = "dpEndDate";
            dpEndDate.Size = new Size(257, 30);
            dpEndDate.TabIndex = 1;
            // 
            // dpStartDate
            // 
            dpStartDate.Location = new Point(307, 17);
            dpStartDate.Name = "dpStartDate";
            dpStartDate.Size = new Size(257, 30);
            dpStartDate.TabIndex = 0;
            // 
            // btnStartUse
            // 
            btnStartUse.Location = new Point(759, 375);
            btnStartUse.Name = "btnStartUse";
            btnStartUse.Size = new Size(301, 89);
            btnStartUse.TabIndex = 5;
            btnStartUse.Text = "开始记分";
            btnStartUse.UseVisualStyleBackColor = true;
            btnStartUse.Click += btnStartUse_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1105, 556);
            Controls.Add(btnStartUse);
            Controls.Add(tabControl1);
            Controls.Add(BtnConfirmExport);
            Controls.Add(chkOverwrite);
            Controls.Add(btnDownloadTemplate);
            Controls.Add(btnImportExcel);
            Name = "MainForm";
            Text = "Form1";
            Load += MainForm_Load;
            tabControl1.ResumeLayout(false);
            学生导出设置.ResumeLayout(false);
            学生导出设置.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnImportExcel;
        private Button btnDownloadTemplate;
        private CheckBox chkOverwrite;
        private Button btnStartUse;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Button BtnConfirmExport;
        private TabPage 学生导出设置;
        private TextBox txtStudentKeyword;
        private ComboBox cmbClass;
        private TextBox txtCourseKeyword;
        private ComboBox cmbLogClass;
        private DateTimePicker dpEndDate;
        private DateTimePicker dpStartDate;
        private Label label2;
        private Label label1;
        private Label label4;
        private Label label3;
        private Label label6;
        private Label label5;
    }
}
