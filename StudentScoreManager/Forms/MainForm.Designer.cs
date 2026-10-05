

using Krypton.Toolkit;

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
            lblTheme = new KryptonLabel();
            cmbTheme = new KryptonComboBox();
            panelLeft = new KryptonPanel();
            lblImportTitle = new KryptonLabel();
            btnDownloadTemplate = new KryptonButton();
            btnImportExcel = new KryptonButton();
            chkOverwrite = new KryptonCheckBox();
            btnCleanupDuplicates = new KryptonButton();
            panelRight = new KryptonPanel();
            tabExport = new TabControl();
            studentsPage = new TabPage();
            label3 = new KryptonLabel();
            cmbClass = new KryptonComboBox();
            label4 = new KryptonLabel();
            txtStudentKeyword = new KryptonTextBox();
            teachingLogPage = new TabPage();
            label1 = new KryptonLabel();
            dpStartDate = new KryptonDateTimePicker();
            label2 = new KryptonLabel();
            dpEndDate = new KryptonDateTimePicker();
            label5 = new KryptonLabel();
            cmbLogClass = new KryptonComboBox();
            label6 = new KryptonLabel();
            cmbCourse = new KryptonComboBox();
            lblExportTitle = new KryptonLabel();
            panelButtons = new KryptonPanel();
            BtnConfirmExport = new KryptonButton();
            btnStartUse = new KryptonButton();
            ((System.ComponentModel.ISupportInitialize)cmbTheme).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelLeft).BeginInit();
            panelLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelRight).BeginInit();
            panelRight.SuspendLayout();
            tabExport.SuspendLayout();
            studentsPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbClass).BeginInit();
            teachingLogPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbLogClass).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbCourse).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelButtons).BeginInit();
            panelButtons.SuspendLayout();
            SuspendLayout();
            // 
            // lblTheme
            // 
            lblTheme.Location = new Point(404, 18);
            lblTheme.Name = "lblTheme";
            lblTheme.Size = new Size(51, 29);
            lblTheme.TabIndex = 5;
            lblTheme.Values.Text = "主题";
            // 
            // cmbTheme
            // 
            cmbTheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTheme.Location = new Point(461, 18);
            cmbTheme.Name = "cmbTheme";
            cmbTheme.Size = new Size(292, 30);
            cmbTheme.TabIndex = 6;
            // 
            // panelLeft
            // 
            panelLeft.Controls.Add(lblImportTitle);
            panelLeft.Controls.Add(btnDownloadTemplate);
            panelLeft.Controls.Add(btnImportExcel);
            panelLeft.Controls.Add(chkOverwrite);
            panelLeft.Controls.Add(btnCleanupDuplicates);
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Location = new Point(0, 0);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new Size(332, 600);
            panelLeft.TabIndex = 0;
            // 
            // lblImportTitle
            // 
            lblImportTitle.Location = new Point(20, 16);
            lblImportTitle.Name = "lblImportTitle";
            lblImportTitle.Size = new Size(90, 29);
            lblImportTitle.TabIndex = 0;
            lblImportTitle.Values.Text = "数据导入";
            // 
            // btnDownloadTemplate
            // 
            btnDownloadTemplate.Location = new Point(20, 58);
            btnDownloadTemplate.Name = "btnDownloadTemplate";
            btnDownloadTemplate.Size = new Size(292, 72);
            btnDownloadTemplate.TabIndex = 1;
            btnDownloadTemplate.Values.DropDownArrowColor = Color.Empty;
            btnDownloadTemplate.Values.Text = "下载导入模板";
            btnDownloadTemplate.Click += btnDownloadTemplate_Click;
            // 
            // btnImportExcel
            // 
            btnImportExcel.Location = new Point(20, 142);
            btnImportExcel.Name = "btnImportExcel";
            btnImportExcel.Size = new Size(292, 72);
            btnImportExcel.TabIndex = 2;
            btnImportExcel.Values.DropDownArrowColor = Color.Empty;
            btnImportExcel.Values.Text = "导入 Excel 数据";
            btnImportExcel.Click += btnImportExcel_Click;
            // 
            // chkOverwrite
            // 
            chkOverwrite.Location = new Point(22, 226);
            chkOverwrite.Name = "chkOverwrite";
            chkOverwrite.Size = new Size(109, 29);
            chkOverwrite.TabIndex = 3;
            chkOverwrite.Values.Text = "覆盖导入";
            // 
            // btnCleanupDuplicates
            // 
            btnCleanupDuplicates.Location = new Point(20, 268);
            btnCleanupDuplicates.Name = "btnCleanupDuplicates";
            btnCleanupDuplicates.Size = new Size(292, 64);
            btnCleanupDuplicates.TabIndex = 4;
            btnCleanupDuplicates.Values.DropDownArrowColor = Color.Empty;
            btnCleanupDuplicates.Values.Text = "清理重复教学日志";
            btnCleanupDuplicates.Click += btnCleanupDuplicates_Click;
            // 
            // panelRight
            // 
            panelRight.Controls.Add(lblTheme);
            panelRight.Controls.Add(tabExport);
            panelRight.Controls.Add(cmbTheme);
            panelRight.Controls.Add(lblExportTitle);
            panelRight.Controls.Add(panelButtons);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(332, 0);
            panelRight.Name = "panelRight";
            panelRight.Size = new Size(788, 600);
            panelRight.TabIndex = 1;
            // 
            // tabExport
            // 
            tabExport.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabExport.Controls.Add(studentsPage);
            tabExport.Controls.Add(teachingLogPage);
            tabExport.Location = new Point(20, 54);
            tabExport.Name = "tabExport";
            tabExport.SelectedIndex = 0;
            tabExport.Size = new Size(748, 446);
            tabExport.TabIndex = 1;
            // 
            // studentsPage
            // 
            studentsPage.Controls.Add(label3);
            studentsPage.Controls.Add(cmbClass);
            studentsPage.Controls.Add(label4);
            studentsPage.Controls.Add(txtStudentKeyword);
            studentsPage.Location = new Point(4, 33);
            studentsPage.Name = "studentsPage";
            studentsPage.Padding = new Padding(3);
            studentsPage.Size = new Size(740, 409);
            studentsPage.TabIndex = 0;
            studentsPage.Text = "学生导出设置";
            // 
            // label3
            // 
            label3.Location = new Point(24, 26);
            label3.Name = "label3";
            label3.Size = new Size(90, 29);
            label3.TabIndex = 0;
            label3.Values.Text = "选择班级";
            // 
            // cmbClass
            // 
            cmbClass.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbClass.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbClass.Items.AddRange(new object[] { "全部" });
            cmbClass.Location = new Point(150, 22);
            cmbClass.Name = "cmbClass";
            cmbClass.Size = new Size(360, 30);
            cmbClass.TabIndex = 1;
            // 
            // label4
            // 
            label4.Location = new Point(24, 76);
            label4.Name = "label4";
            label4.Size = new Size(136, 29);
            label4.TabIndex = 2;
            label4.Values.Text = "搜索姓名/学号";
            // 
            // txtStudentKeyword
            // 
            txtStudentKeyword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtStudentKeyword.Location = new Point(150, 72);
            txtStudentKeyword.Name = "txtStudentKeyword";
            txtStudentKeyword.Size = new Size(360, 31);
            txtStudentKeyword.TabIndex = 3;
            // 
            // teachingLogPage
            // 
            teachingLogPage.Controls.Add(label1);
            teachingLogPage.Controls.Add(dpStartDate);
            teachingLogPage.Controls.Add(label2);
            teachingLogPage.Controls.Add(dpEndDate);
            teachingLogPage.Controls.Add(label5);
            teachingLogPage.Controls.Add(cmbLogClass);
            teachingLogPage.Controls.Add(label6);
            teachingLogPage.Controls.Add(cmbCourse);
            teachingLogPage.Location = new Point(4, 33);
            teachingLogPage.Name = "teachingLogPage";
            teachingLogPage.Padding = new Padding(3);
            teachingLogPage.Size = new Size(740, 409);
            teachingLogPage.TabIndex = 1;
            teachingLogPage.Text = "日志导出设置";
            // 
            // label1
            // 
            label1.Location = new Point(24, 26);
            label1.Name = "label1";
            label1.Size = new Size(90, 29);
            label1.TabIndex = 0;
            label1.Values.Text = "开始时间";
            // 
            // dpStartDate
            // 
            dpStartDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dpStartDate.Checked = false;
            dpStartDate.Location = new Point(150, 22);
            dpStartDate.Name = "dpStartDate";
            dpStartDate.ShowCheckBox = true;
            dpStartDate.Size = new Size(360, 30);
            dpStartDate.TabIndex = 1;
            // 
            // label2
            // 
            label2.Location = new Point(24, 72);
            label2.Name = "label2";
            label2.Size = new Size(90, 29);
            label2.TabIndex = 2;
            label2.Values.Text = "结束时间";
            // 
            // dpEndDate
            // 
            dpEndDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dpEndDate.Location = new Point(150, 68);
            dpEndDate.Name = "dpEndDate";
            dpEndDate.ShowCheckBox = true;
            dpEndDate.Size = new Size(360, 30);
            dpEndDate.TabIndex = 3;
            dpEndDate.ValueNullable = new DateTime(2026, 9, 28, 0, 0, 0, 0);
            // 
            // label5
            // 
            label5.Location = new Point(24, 118);
            label5.Name = "label5";
            label5.Size = new Size(90, 29);
            label5.TabIndex = 4;
            label5.Values.Text = "选择班级";
            // 
            // cmbLogClass
            // 
            cmbLogClass.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbLogClass.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLogClass.Items.AddRange(new object[] { "全部" });
            cmbLogClass.Location = new Point(150, 114);
            cmbLogClass.Name = "cmbLogClass";
            cmbLogClass.Size = new Size(360, 30);
            cmbLogClass.TabIndex = 5;
            // 
            // label6
            // 
            label6.Location = new Point(24, 164);
            label6.Name = "label6";
            label6.Size = new Size(90, 29);
            label6.TabIndex = 6;
            label6.Values.Text = "课程名称";
            // 
            // cmbCourse
            // 
            cmbCourse.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbCourse.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCourse.Items.AddRange(new object[] { "全部" });
            cmbCourse.Location = new Point(150, 160);
            cmbCourse.Name = "cmbCourse";
            cmbCourse.Size = new Size(360, 30);
            cmbCourse.TabIndex = 7;
            // 
            // lblExportTitle
            // 
            lblExportTitle.Location = new Point(20, 16);
            lblExportTitle.Name = "lblExportTitle";
            lblExportTitle.Size = new Size(90, 29);
            lblExportTitle.TabIndex = 0;
            lblExportTitle.Values.Text = "数据导出";
            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(BtnConfirmExport);
            panelButtons.Controls.Add(btnStartUse);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 510);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(788, 90);
            panelButtons.TabIndex = 2;
            // 
            // BtnConfirmExport
            // 
            BtnConfirmExport.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            BtnConfirmExport.Location = new Point(20, 14);
            BtnConfirmExport.Name = "BtnConfirmExport";
            BtnConfirmExport.Size = new Size(350, 62);
            BtnConfirmExport.TabIndex = 0;
            BtnConfirmExport.Values.DropDownArrowColor = Color.Empty;
            BtnConfirmExport.Values.Text = "确认导出";
            BtnConfirmExport.Click += BtnConfirmExport_Click;
            // 
            // btnStartUse
            // 
            btnStartUse.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnStartUse.Location = new Point(418, 14);
            btnStartUse.Name = "btnStartUse";
            btnStartUse.Size = new Size(350, 62);
            btnStartUse.TabIndex = 1;
            btnStartUse.Values.DropDownArrowColor = Color.Empty;
            btnStartUse.Values.Text = "开始记分";
            btnStartUse.Click += btnStartUse_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1120, 600);
            Controls.Add(panelRight);
            Controls.Add(panelLeft);
            FormTitleAlign = PaletteRelativeAlign.Center;
            MinimumSize = new Size(980, 560);
            Name = "MainForm";
            Text = "数据导入导出";
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)cmbTheme).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelLeft).EndInit();
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)panelRight).EndInit();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            tabExport.ResumeLayout(false);
            studentsPage.ResumeLayout(false);
            studentsPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)cmbClass).EndInit();
            teachingLogPage.ResumeLayout(false);
            teachingLogPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)cmbLogClass).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbCourse).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelButtons).EndInit();
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private KryptonLabel lblTheme;
        private KryptonComboBox cmbTheme;
        private KryptonPanel panelLeft;
        private KryptonPanel panelRight;
        private KryptonPanel panelButtons;
        private KryptonLabel lblImportTitle;
        private KryptonLabel lblExportTitle;
        private KryptonButton btnImportExcel;
        private KryptonButton btnDownloadTemplate;
        private KryptonButton btnCleanupDuplicates;
        private KryptonButton BtnConfirmExport;
        private KryptonButton btnStartUse;
        private KryptonCheckBox chkOverwrite;
        private TabControl tabExport;
        private TabPage studentsPage;
        private TabPage teachingLogPage;
        private KryptonTextBox txtStudentKeyword;
        private KryptonComboBox cmbClass;
        private KryptonComboBox cmbCourse;
        private KryptonComboBox cmbLogClass;
        private KryptonDateTimePicker dpEndDate;
        private KryptonDateTimePicker dpStartDate;
        private KryptonLabel label1;
        private KryptonLabel label2;
        private KryptonLabel label3;
        private KryptonLabel label4;
        private KryptonLabel label5;
        private KryptonLabel label6;
    }
}