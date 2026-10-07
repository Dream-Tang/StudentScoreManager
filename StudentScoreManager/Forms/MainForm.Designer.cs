
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
        ///  Clean up any resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

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
            panelFilter = new KryptonPanel();
            lblSecStudent = new KryptonLabel();
            label3 = new KryptonLabel();
            cmbClass = new KryptonComboBox();
            label4 = new KryptonLabel();
            txtStudentKeyword = new KryptonTextBox();
            label1 = new KryptonLabel();
            dpStartDate = new KryptonDateTimePicker();
            label2 = new KryptonLabel();
            dpEndDate = new KryptonDateTimePicker();
            label6 = new KryptonLabel();
            cmbCourse = new KryptonComboBox();
            lblSecOption = new KryptonLabel();
            chkExportDetails = new KryptonCheckBox();
            lblExportTitle = new KryptonLabel();
            panelButtons = new KryptonPanel();
            BtnConfirmExport = new KryptonButton();
            btnStartUse = new KryptonButton();
            sepOption = new KryptonSeparator();
            sepStudent = new KryptonSeparator();
            ((System.ComponentModel.ISupportInitialize)cmbTheme).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelLeft).BeginInit();
            panelLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelRight).BeginInit();
            panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelFilter).BeginInit();
            panelFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbClass).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbCourse).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelButtons).BeginInit();
            panelButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sepOption).BeginInit();
            ((System.ComponentModel.ISupportInitialize)sepStudent).BeginInit();
            SuspendLayout();
            // 
            // lblTheme
            // 
            lblTheme.Location = new Point(20, 356);
            lblTheme.Name = "lblTheme";
            lblTheme.Size = new Size(51, 29);
            lblTheme.TabIndex = 5;
            lblTheme.Values.Text = "主题";
            // 
            // cmbTheme
            // 
            cmbTheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTheme.Location = new Point(20, 388);
            cmbTheme.Name = "cmbTheme";
            cmbTheme.Size = new Size(292, 30);
            cmbTheme.TabIndex = 6;
            // 
            // panelLeft
            // 
            panelLeft.Controls.Add(lblTheme);
            panelLeft.Controls.Add(cmbTheme);
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
            panelRight.Controls.Add(panelFilter);
            panelRight.Controls.Add(lblExportTitle);
            panelRight.Controls.Add(panelButtons);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(332, 0);
            panelRight.Name = "panelRight";
            panelRight.Size = new Size(788, 600);
            panelRight.TabIndex = 1;
            // 
            // panelFilter
            // 
            panelFilter.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelFilter.Controls.Add(lblSecStudent);
            panelFilter.Controls.Add(sepStudent);
            panelFilter.Controls.Add(label3);
            panelFilter.Controls.Add(cmbClass);
            panelFilter.Controls.Add(label4);
            panelFilter.Controls.Add(txtStudentKeyword);
            panelFilter.Controls.Add(label1);
            panelFilter.Controls.Add(dpStartDate);
            panelFilter.Controls.Add(label2);
            panelFilter.Controls.Add(dpEndDate);
            panelFilter.Controls.Add(label6);
            panelFilter.Controls.Add(cmbCourse);
            panelFilter.Controls.Add(lblSecOption);
            panelFilter.Controls.Add(sepOption);
            panelFilter.Controls.Add(chkExportDetails);
            panelFilter.Location = new Point(20, 54);
            panelFilter.Name = "panelFilter";
            panelFilter.Size = new Size(748, 446);
            panelFilter.TabIndex = 1;
            // 
            // lblSecStudent
            // 
            lblSecStudent.Location = new Point(10, 4);
            lblSecStudent.Name = "lblSecStudent";
            lblSecStudent.Size = new Size(168, 29);
            lblSecStudent.TabIndex = 0;
            lblSecStudent.Values.Text = "按照筛选导出数据";
            // 
            // label3
            // 
            label3.Location = new Point(24, 138);
            label3.Name = "label3";
            label3.Size = new Size(90, 29);
            label3.TabIndex = 2;
            label3.Values.Text = "选择班级";
            // 
            // cmbClass
            // 
            cmbClass.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbClass.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbClass.Items.AddRange(new object[] { "全部" });
            cmbClass.Location = new Point(160, 134);
            cmbClass.Name = "cmbClass";
            cmbClass.Size = new Size(440, 30);
            cmbClass.TabIndex = 3;
            // 
            // label4
            // 
            label4.Location = new Point(24, 74);
            label4.Name = "label4";
            label4.Size = new Size(136, 29);
            label4.TabIndex = 4;
            label4.Values.Text = "搜索姓名/学号";
            // 
            // txtStudentKeyword
            // 
            txtStudentKeyword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtStudentKeyword.Location = new Point(160, 70);
            txtStudentKeyword.Name = "txtStudentKeyword";
            txtStudentKeyword.Size = new Size(440, 31);
            txtStudentKeyword.TabIndex = 5;
            // 
            // label1
            // 
            label1.Location = new Point(24, 174);
            label1.Name = "label1";
            label1.Size = new Size(90, 29);
            label1.TabIndex = 8;
            label1.Values.Text = "开始时间";
            // 
            // dpStartDate
            // 
            dpStartDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dpStartDate.Checked = false;
            dpStartDate.Location = new Point(160, 170);
            dpStartDate.Name = "dpStartDate";
            dpStartDate.ShowCheckBox = true;
            dpStartDate.Size = new Size(440, 30);
            dpStartDate.TabIndex = 9;
            // 
            // label2
            // 
            label2.Location = new Point(24, 210);
            label2.Name = "label2";
            label2.Size = new Size(90, 29);
            label2.TabIndex = 10;
            label2.Values.Text = "结束时间";
            // 
            // dpEndDate
            // 
            dpEndDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dpEndDate.Location = new Point(160, 206);
            dpEndDate.Name = "dpEndDate";
            dpEndDate.ShowCheckBox = true;
            dpEndDate.Size = new Size(440, 30);
            dpEndDate.TabIndex = 11;
            dpEndDate.ValueNullable = new DateTime(2026, 9, 28, 0, 0, 0, 0);
            // 
            // label6
            // 
            label6.Location = new Point(24, 246);
            label6.Name = "label6";
            label6.Size = new Size(90, 29);
            label6.TabIndex = 14;
            label6.Values.Text = "课程名称";
            // 
            // cmbCourse
            // 
            cmbCourse.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbCourse.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCourse.Items.AddRange(new object[] { "全部" });
            cmbCourse.Location = new Point(160, 242);
            cmbCourse.Name = "cmbCourse";
            cmbCourse.Size = new Size(440, 30);
            cmbCourse.TabIndex = 15;
            // 
            // lblSecOption
            // 
            lblSecOption.Location = new Point(10, 322);
            lblSecOption.Name = "lblSecOption";
            lblSecOption.Size = new Size(90, 29);
            lblSecOption.TabIndex = 16;
            lblSecOption.Values.Text = "导出选项";
            // 
            // chkExportDetails
            // 
            chkExportDetails.Checked = true;
            chkExportDetails.CheckState = CheckState.Checked;
            chkExportDetails.Location = new Point(122, 344);
            chkExportDetails.Name = "chkExportDetails";
            chkExportDetails.Size = new Size(478, 29);
            chkExportDetails.TabIndex = 18;
            chkExportDetails.Values.Text = "导出满足筛选的成绩明细（数据量大，可按需关闭）";
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
            // sepOption
            // 
            sepOption.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            sepOption.Location = new Point(10, 318);
            sepOption.Name = "sepOption";
            sepOption.Size = new Size(720, 4);
            sepOption.TabIndex = 17;
            sepOption.TabStop = false;
            // 
            // sepStudent
            // 
            sepStudent.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            sepStudent.Location = new Point(10, 36);
            sepStudent.Name = "sepStudent";
            sepStudent.Size = new Size(720, 4);
            sepStudent.TabIndex = 1;
            sepStudent.TabStop = false;
            sepStudent.ToolTipValues.EnableToolTips = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1120, 600);
            Controls.Add(panelRight);
            Controls.Add(panelLeft);
            MinimumSize = new Size(980, 560);
            Name = "MainForm";
            Text = "数据导入导出（联系作者：tangjun2079@163.com）";
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)cmbTheme).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelLeft).EndInit();
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)panelRight).EndInit();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)panelFilter).EndInit();
            panelFilter.ResumeLayout(false);
            panelFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)cmbClass).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbCourse).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelButtons).EndInit();
            panelButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)sepOption).EndInit();
            ((System.ComponentModel.ISupportInitialize)sepStudent).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private KryptonLabel lblTheme;
        private KryptonComboBox cmbTheme;
        private KryptonPanel panelLeft;
        private KryptonPanel panelRight;
        private KryptonPanel panelFilter;
        private KryptonPanel panelButtons;
        private KryptonLabel lblImportTitle;
        private KryptonLabel lblExportTitle;
        private KryptonButton btnImportExcel;
        private KryptonButton btnDownloadTemplate;
        private KryptonButton btnCleanupDuplicates;
        private KryptonButton BtnConfirmExport;
        private KryptonButton btnStartUse;
        private KryptonCheckBox chkOverwrite;
        private KryptonCheckBox chkExportDetails;
        private KryptonTextBox txtStudentKeyword;
        private KryptonComboBox cmbClass;
        private KryptonComboBox cmbCourse;
        private KryptonDateTimePicker dpEndDate;
        private KryptonDateTimePicker dpStartDate;
        private KryptonLabel label1;
        private KryptonLabel label2;
        private KryptonLabel label3;
        private KryptonLabel label4;
        private KryptonLabel label6;
        private KryptonLabel lblSecStudent;
        private KryptonLabel lblSecOption;
        private KryptonSeparator sepStudent;
        private KryptonSeparator sepOption;
    }
}