


namespace StudentScoreManager.Forms
{
    partial class Form1
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
            lblTheme = new Krypton.Toolkit.KryptonLabel();
            cmbTheme = new Krypton.Toolkit.KryptonComboBox();
            pnlTop = new Krypton.Toolkit.KryptonPanel();
            lblDate = new Krypton.Toolkit.KryptonLabel();
            dtpDate = new DateTimePicker();
            btnRefresh = new Krypton.Toolkit.KryptonButton();
            btnMappingEditor = new Krypton.Toolkit.KryptonButton();
            splitMain = new SplitContainer();
            flpCourses = new DoubleBufferedFlowLayoutPanel();
            dgvScoreDetail = new DataGridView();
            pnlHeader = new Krypton.Toolkit.KryptonPanel();
            lblCourseTitle = new Label();
            lblSaveState = new Label();
            pnlActions = new Krypton.Toolkit.KryptonPanel();
            btnPresent = new Krypton.Toolkit.KryptonButton();
            btnClear = new Krypton.Toolkit.KryptonButton();
            btnSave = new Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)cmbTheme).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlTop).BeginInit();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvScoreDetail).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlActions).BeginInit();
            pnlActions.SuspendLayout();
            SuspendLayout();
            // 
            // lblTheme
            // 
            lblTheme.Location = new Point(360, 22);
            lblTheme.Name = "lblTheme";
            lblTheme.Size = new Size(51, 29);
            lblTheme.TabIndex = 4;
            lblTheme.Values.Text = "主题";
            // 
            // cmbTheme
            // 
            cmbTheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTheme.Location = new Point(415, 18);
            cmbTheme.Name = "cmbTheme";
            cmbTheme.Size = new Size(200, 30);
            cmbTheme.TabIndex = 5;
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(lblTheme);
            pnlTop.Controls.Add(cmbTheme);
            pnlTop.Controls.Add(lblDate);
            pnlTop.Controls.Add(dtpDate);
            pnlTop.Controls.Add(btnRefresh);
            pnlTop.Controls.Add(btnMappingEditor);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1395, 68);
            pnlTop.TabIndex = 0;
            // 
            // lblDate
            // 
            lblDate.Location = new Point(20, 22);
            lblDate.Name = "lblDate";
            lblDate.Size = new Size(90, 29);
            lblDate.TabIndex = 0;
            lblDate.Values.Text = "授课日期";
            // 
            // dtpDate
            // 
            dtpDate.CalendarFont = new Font("Microsoft YaHei UI", 10.5F);
            dtpDate.CustomFormat = "yyyy年MM月dd日";
            dtpDate.Font = new Font("Microsoft YaHei UI", 10.5F);
            dtpDate.Format = DateTimePickerFormat.Custom;
            dtpDate.Location = new Point(106, 20);
            dtpDate.Name = "dtpDate";
            dtpDate.Size = new Size(220, 34);
            dtpDate.TabIndex = 1;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Location = new Point(1115, 15);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(120, 40);
            btnRefresh.TabIndex = 2;
            btnRefresh.Values.DropDownArrowColor = Color.Empty;
            btnRefresh.Values.Text = "刷新";
            // 
            // btnMappingEditor
            // 
            btnMappingEditor.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMappingEditor.Location = new Point(1251, 15);
            btnMappingEditor.Name = "btnMappingEditor";
            btnMappingEditor.Size = new Size(124, 40);
            btnMappingEditor.TabIndex = 3;
            btnMappingEditor.Values.DropDownArrowColor = Color.Empty;
            btnMappingEditor.Values.Text = "评分映射设置";
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.FixedPanel = FixedPanel.Panel1;
            splitMain.Location = new Point(0, 68);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(flpCourses);
            splitMain.Panel1.Padding = new Padding(10, 8, 4, 8);
            splitMain.Panel1MinSize = 260;
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(dgvScoreDetail);
            splitMain.Panel2.Controls.Add(pnlHeader);
            splitMain.Panel2.Controls.Add(pnlActions);
            splitMain.Panel2.Padding = new Padding(8, 8, 10, 8);
            splitMain.Panel2MinSize = 520;
            splitMain.Size = new Size(1395, 952);
            splitMain.SplitterDistance = 336;
            splitMain.SplitterWidth = 8;
            splitMain.TabIndex = 1;
            // 
            // flpCourses
            // 
            flpCourses.AutoScroll = true;
            flpCourses.Dock = DockStyle.Fill;
            flpCourses.FlowDirection = FlowDirection.TopDown;
            flpCourses.Location = new Point(10, 8);
            flpCourses.Name = "flpCourses";
            flpCourses.Padding = new Padding(2, 2, 2, 8);
            flpCourses.Size = new Size(322, 936);
            flpCourses.TabIndex = 0;
            flpCourses.WrapContents = false;
            // 
            // dgvScoreDetail
            // 
            dgvScoreDetail.AllowUserToDeleteRows = false;
            dgvScoreDetail.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvScoreDetail.Dock = DockStyle.Fill;
            dgvScoreDetail.Location = new Point(8, 64);
            dgvScoreDetail.Name = "dgvScoreDetail";
            dgvScoreDetail.RowHeadersVisible = false;
            dgvScoreDetail.RowHeadersWidth = 62;
            dgvScoreDetail.Size = new Size(1033, 812);
            dgvScoreDetail.TabIndex = 2;
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(lblCourseTitle);
            pnlHeader.Controls.Add(lblSaveState);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(8, 8);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1033, 56);
            pnlHeader.TabIndex = 0;
            // 
            // lblCourseTitle
            // 
            lblCourseTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblCourseTitle.Location = new Point(12, 14);
            lblCourseTitle.Name = "lblCourseTitle";
            lblCourseTitle.Size = new Size(675, 30);
            lblCourseTitle.TabIndex = 0;
            lblCourseTitle.Text = "请选择左侧课程";
            // 
            // lblSaveState
            // 
            lblSaveState.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSaveState.Location = new Point(911, 14);
            lblSaveState.Name = "lblSaveState";
            lblSaveState.Size = new Size(110, 30);
            lblSaveState.TabIndex = 1;
            lblSaveState.Text = "● 已保存";
            // 
            // pnlActions
            // 
            pnlActions.Controls.Add(btnPresent);
            pnlActions.Controls.Add(btnClear);
            pnlActions.Controls.Add(btnSave);
            pnlActions.Dock = DockStyle.Bottom;
            pnlActions.Location = new Point(8, 876);
            pnlActions.Name = "pnlActions";
            pnlActions.Size = new Size(1033, 68);
            pnlActions.TabIndex = 1;
            // 
            // btnPresent
            // 
            btnPresent.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPresent.Location = new Point(645, 14);
            btnPresent.Name = "btnPresent";
            btnPresent.Size = new Size(120, 40);
            btnPresent.TabIndex = 0;
            btnPresent.Values.DropDownArrowColor = Color.Empty;
            btnPresent.Values.Text = "全部出勤";
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClear.Location = new Point(775, 14);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(120, 40);
            btnClear.TabIndex = 1;
            btnClear.Values.DropDownArrowColor = Color.Empty;
            btnClear.Values.Text = "清除选定行";
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Location = new Point(905, 14);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(120, 40);
            btnSave.TabIndex = 2;
            btnSave.Values.DropDownArrowColor = Color.Empty;
            btnSave.Values.Text = "保存评分";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1395, 1020);
            Controls.Add(splitMain);
            Controls.Add(pnlTop);
            MinimumSize = new Size(1080, 700);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "课堂评分";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)cmbTheme).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlTop).EndInit();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvScoreDetail).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlActions).EndInit();
            pnlActions.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Krypton.Toolkit.KryptonLabel lblTheme;
        private Krypton.Toolkit.KryptonComboBox cmbTheme;
        private Krypton.Toolkit.KryptonPanel pnlTop;
        private Krypton.Toolkit.KryptonLabel lblDate;
        private DateTimePicker dtpDate;
        private Krypton.Toolkit.KryptonButton btnRefresh;
        private Krypton.Toolkit.KryptonButton btnMappingEditor;
        private SplitContainer splitMain;
        private DoubleBufferedFlowLayoutPanel flpCourses;
        private DataGridView dgvScoreDetail;
        private Krypton.Toolkit.KryptonPanel pnlHeader;
        private Label lblCourseTitle;
        private Label lblSaveState;
        private Krypton.Toolkit.KryptonPanel pnlActions;
        private Krypton.Toolkit.KryptonButton btnPresent;
        private Krypton.Toolkit.KryptonButton btnClear;
        private Krypton.Toolkit.KryptonButton btnSave;
    }
}