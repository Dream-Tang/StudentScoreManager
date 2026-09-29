

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
            button1 = new Button();
            dgvLogInfo = new DataGridView();
            teachingHours = new DataGridViewTextBoxColumn();
            CourseCode = new DataGridViewTextBoxColumn();
            CourseName = new DataGridViewTextBoxColumn();
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
            dtpTeachingDate.CalendarFont = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dtpTeachingDate.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            dtpTeachingDate.Format = DateTimePickerFormat.Short;
            dtpTeachingDate.Location = new Point(113, 39);
            dtpTeachingDate.Name = "dtpTeachingDate";
            dtpTeachingDate.Size = new Size(282, 38);
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
            // button1
            // 
            button1.Location = new Point(474, 65);
            button1.Name = "button1";
            button1.Size = new Size(112, 34);
            button1.TabIndex = 4;
            button1.Text = "刷新";
            button1.UseVisualStyleBackColor = true;
            // 
            // dgvLogInfo
            // 
            dgvLogInfo.AllowUserToDeleteRows = false;
            dgvLogInfo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLogInfo.Columns.AddRange(new DataGridViewColumn[] { teachingHours, CourseCode, CourseName, ClasssName, ClassRoom, TeachingContent });
            dgvLogInfo.Location = new Point(25, 114);
            dgvLogInfo.Name = "dgvLogInfo";
            dgvLogInfo.RowHeadersVisible = false;
            dgvLogInfo.RowHeadersWidth = 62;
            dgvLogInfo.Size = new Size(1268, 221);
            dgvLogInfo.TabIndex = 5;
            // 
            // teachingHours
            // 
            teachingHours.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            teachingHours.DataPropertyName = "TeachingHours";
            teachingHours.HeaderText = "节次";
            teachingHours.MinimumWidth = 8;
            teachingHours.Name = "teachingHours";
            teachingHours.Width = 82;
            // 
            // CourseCode
            // 
            CourseCode.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            CourseCode.HeaderText = "课程编号";
            CourseCode.MinimumWidth = 8;
            CourseCode.Name = "CourseCode";
            CourseCode.Width = 118;
            // 
            // CourseName
            // 
            CourseName.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            CourseName.DataPropertyName = "CourseName";
            CourseName.HeaderText = "课程名称";
            CourseName.MinimumWidth = 8;
            CourseName.Name = "CourseName";
            CourseName.Width = 118;
            // 
            // ClasssName
            // 
            ClasssName.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            ClasssName.DataPropertyName = "ClassName";
            ClasssName.HeaderText = "班级名称";
            ClasssName.MinimumWidth = 8;
            ClasssName.Name = "ClasssName";
            ClasssName.Width = 118;
            // 
            // ClassRoom
            // 
            ClassRoom.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            ClassRoom.DataPropertyName = "Classroom";
            ClassRoom.HeaderText = "教室";
            ClassRoom.MinimumWidth = 8;
            ClassRoom.Name = "ClassRoom";
            ClassRoom.Width = 82;
            // 
            // TeachingContent
            // 
            TeachingContent.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            TeachingContent.DataPropertyName = "Content";
            TeachingContent.HeaderText = "教学内容";
            TeachingContent.MinimumWidth = 8;
            TeachingContent.Name = "TeachingContent";
            // 
            // dgvScoreDetail
            // 
            dgvScoreDetail.AllowUserToDeleteRows = false;
            dgvScoreDetail.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvScoreDetail.Location = new Point(25, 355);
            dgvScoreDetail.Name = "dgvScoreDetail";
            dgvScoreDetail.RowHeadersVisible = false;
            dgvScoreDetail.RowHeadersWidth = 62;
            dgvScoreDetail.Size = new Size(1268, 483);
            dgvScoreDetail.TabIndex = 6;
            // 
            // button2
            // 
            button2.Location = new Point(767, 25);
            button2.Name = "button2";
            button2.Size = new Size(99, 74);
            button2.TabIndex = 7;
            button2.Text = "保存数据";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(474, 25);
            button3.Name = "button3";
            button3.Size = new Size(112, 34);
            button3.TabIndex = 8;
            button3.Text = "全部出勤";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(611, 25);
            button4.Name = "button4";
            button4.Size = new Size(136, 34);
            button4.TabIndex = 9;
            button4.Text = "清除选定行";
            button4.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            button5.Location = new Point(611, 65);
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
        private Button button1;
        private DataGridView dgvLogInfo;
        private DataGridView dgvScoreDetail;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private DataGridViewTextBoxColumn teachingHours;
        private DataGridViewTextBoxColumn CourseCode;
        private DataGridViewTextBoxColumn CourseName;
        private DataGridViewTextBoxColumn ClasssName;
        private DataGridViewTextBoxColumn ClassRoom;
        private DataGridViewTextBoxColumn TeachingContent;
    }
}