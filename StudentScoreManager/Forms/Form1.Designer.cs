
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
            sqliteCommand1 = new Microsoft.Data.Sqlite.SqliteCommand();
            dtpTeachingDate = new DateTimePicker();
            label1 = new Label();
            label2 = new Label();
            cmbCourseCode = new ComboBox();
            button1 = new Button();
            dgvLogInfo = new DataGridView();
            CourseName = new DataGridViewTextBoxColumn();
            teachingHours = new DataGridViewTextBoxColumn();
            ClasssName = new DataGridViewTextBoxColumn();
            ClassRoom = new DataGridViewTextBoxColumn();
            TeachingContent = new DataGridViewTextBoxColumn();
            dgvScoreDetail = new DataGridView();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvLogInfo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvScoreDetail).BeginInit();
            SuspendLayout();
            // 
            // sqliteCommand1
            // 
            sqliteCommand1.CommandTimeout = 30;
            sqliteCommand1.Connection = null;
            sqliteCommand1.Transaction = null;
            sqliteCommand1.UpdatedRowSource = System.Data.UpdateRowSource.None;
            // 
            // dtpTeachingDate
            // 
            dtpTeachingDate.Location = new Point(178, 29);
            dtpTeachingDate.Name = "dtpTeachingDate";
            dtpTeachingDate.Size = new Size(261, 30);
            dtpTeachingDate.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(25, 39);
            label1.Name = "label1";
            label1.Size = new Size(82, 24);
            label1.TabIndex = 1;
            label1.Text = "授课日期";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(25, 92);
            label2.Name = "label2";
            label2.Size = new Size(82, 24);
            label2.TabIndex = 2;
            label2.Text = "课程选择";
            // 
            // cmbCourseCode
            // 
            cmbCourseCode.FormattingEnabled = true;
            cmbCourseCode.Location = new Point(178, 99);
            cmbCourseCode.Name = "cmbCourseCode";
            cmbCourseCode.Size = new Size(261, 32);
            cmbCourseCode.TabIndex = 3;
            // 
            // button1
            // 
            button1.Location = new Point(25, 205);
            button1.Name = "button1";
            button1.Size = new Size(112, 34);
            button1.TabIndex = 4;
            button1.Text = "刷新";
            button1.UseVisualStyleBackColor = true;
            // 
            // dgvLogInfo
            // 
            dgvLogInfo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLogInfo.Columns.AddRange(new DataGridViewColumn[] { CourseName, teachingHours, ClasssName, ClassRoom, TeachingContent });
            dgvLogInfo.Location = new Point(471, 18);
            dgvLogInfo.Name = "dgvLogInfo";
            dgvLogInfo.RowHeadersVisible = false;
            dgvLogInfo.RowHeadersWidth = 62;
            dgvLogInfo.Size = new Size(776, 221);
            dgvLogInfo.TabIndex = 5;
            // 
            // CourseName
            // 
            CourseName.DataPropertyName = "CourseName";
            CourseName.HeaderText = "课程名称";
            CourseName.MinimumWidth = 8;
            CourseName.Name = "CourseName";
            CourseName.Width = 150;
            // 
            // teachingHours
            // 
            teachingHours.DataPropertyName = "TeachingHours";
            teachingHours.HeaderText = "节次";
            teachingHours.MinimumWidth = 8;
            teachingHours.Name = "teachingHours";
            teachingHours.Width = 150;
            // 
            // ClasssName
            // 
            ClasssName.DataPropertyName = "ClassName";
            ClasssName.HeaderText = "班级名称";
            ClasssName.MinimumWidth = 8;
            ClasssName.Name = "ClasssName";
            ClasssName.Width = 150;
            // 
            // ClassRoom
            // 
            ClassRoom.DataPropertyName = "Classroom";
            ClassRoom.HeaderText = "教室";
            ClassRoom.MinimumWidth = 8;
            ClassRoom.Name = "ClassRoom";
            ClassRoom.Width = 150;
            // 
            // TeachingContent
            // 
            TeachingContent.DataPropertyName = "Content";
            TeachingContent.HeaderText = "教学内容";
            TeachingContent.MinimumWidth = 8;
            TeachingContent.Name = "TeachingContent";
            TeachingContent.Width = 150;
            // 
            // dgvScoreDetail
            // 
            dgvScoreDetail.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvScoreDetail.Location = new Point(25, 265);
            dgvScoreDetail.Name = "dgvScoreDetail";
            dgvScoreDetail.RowHeadersVisible = false;
            dgvScoreDetail.RowHeadersWidth = 62;
            dgvScoreDetail.Size = new Size(1222, 573);
            dgvScoreDetail.TabIndex = 6;
            // 
            // button2
            // 
            button2.Location = new Point(303, 165);
            button2.Name = "button2";
            button2.Size = new Size(112, 34);
            button2.TabIndex = 7;
            button2.Text = "保存数据";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(25, 165);
            button3.Name = "button3";
            button3.Size = new Size(112, 34);
            button3.TabIndex = 8;
            button3.Text = "全部出勤";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(162, 165);
            button4.Name = "button4";
            button4.Size = new Size(112, 34);
            button4.TabIndex = 9;
            button4.Text = "清除选定行";
            button4.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            button5.Location = new Point(303, 205);
            button5.Name = "button5";
            button5.Size = new Size(136, 34);
            button5.TabIndex = 10;
            button5.Text = "评分映射设置";
            button5.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1305, 890);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(dgvScoreDetail);
            Controls.Add(dgvLogInfo);
            Controls.Add(button1);
            Controls.Add(cmbCourseCode);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(dtpTeachingDate);
            Name = "Form1";
            Text = "课堂评分";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dgvLogInfo).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvScoreDetail).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Microsoft.Data.Sqlite.SqliteCommand sqliteCommand1;
        private DateTimePicker dtpTeachingDate;
        private Label label1;
        private Label label2;
        private ComboBox cmbCourseCode;
        private Button button1;
        private DataGridView dgvLogInfo;
        private DataGridViewTextBoxColumn CourseName;
        private DataGridViewTextBoxColumn teachingHours;
        private DataGridViewTextBoxColumn ClasssName;
        private DataGridViewTextBoxColumn ClassRoom;
        private DataGridViewTextBoxColumn TeachingContent;
        private DataGridView dgvScoreDetail;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
    }
}