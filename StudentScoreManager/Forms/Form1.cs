using StudentScoreManager.Models;
using StudentScoreManager.Services;
using System.Diagnostics;

namespace StudentScoreManager.Forms
{
    public partial class Form1 : Form
    {
        // Form1 类字段区
        private readonly DbHelper _db;

        // ==== TeachingLogs 表列名 ====
        private const string T_LogID = "LogID";
        private const string T_Date = "TeachingDate";
        private const string T_CourseCode = "CourseCode";
        private const string T_CourseName = "CourseName";
        private const string T_TeachingHours = "TeachingHours";
        private const string T_ClassName = "ClassName";
        private const string T_Classroom = "Classroom";
        private const string T_Content = "TeachingContent";

        // ==== Students 表列名 ====
        private const string S_StudentID = "StudentID";
        private const string S_StudentName = "StudentName";
        private const string S_ClassName = "ClassName";


        public Form1()
        {
            InitializeComponent();
            _db = new DbHelper("scores.db");
        }

        private void Form1_Load(object sender, EventArgs e)
        {   // 窗体初始化
            InitializePage();
        }

        private void InitializePage()
        {
            // ==========================================
            // 1. 设置授课日期默认值为今天
            // ==========================================
            dtpTeachingDate.Value = DateTime.Today;

            // ==========================================
            // 2. 配置上方的【课程信息确认表】(只读)
            // ==========================================
            // dgvLogInfo 是上方确认课程的 DataGridView 控件名
            dgvLogInfo.EnableHeadersVisualStyles = false; // 关闭视觉样式，为后续改标题颜色做准备
            dgvLogInfo.AutoGenerateColumns = false;
            dgvLogInfo.ReadOnly = true;
            dgvLogInfo.AllowUserToAddRows = false; // 隐藏最下方带星号的新增行
            dgvLogInfo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 添加列 (DataPropertyName 对应 TeachingLogs 表中的字段名，请根据你实际的数据库字段名修改)
            //dgvLogInfo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "课程名称", DataPropertyName = "CourseName", Width = 150 });
            //dgvLogInfo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "班级名称", DataPropertyName = "ClassName", Width = 120 });
            //dgvLogInfo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "教室", DataPropertyName = "Classroom", Width = 80 });
            //dgvLogInfo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "节次", DataPropertyName = "TeachingHours", Width = 60 });
            //dgvLogInfo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "教学内容", DataPropertyName = "TeachingContent" }); // 教学内容内容较多，不指定宽度让它自适应拉伸

            // ==========================================
            // 3. 配置下方的【学生评分明细表】(可编辑)
            // ==========================================
            // dgvScoreDetail 是下方评分的 DataGridView 控件名
            dgvScoreDetail.EnableHeadersVisualStyles = false;
            dgvScoreDetail.AutoGenerateColumns = false;
            dgvScoreDetail.AllowUserToAddRows = false;
            dgvScoreDetail.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 隐藏默认左侧行号，准备用代码绘制序号
            dgvScoreDetail.RowHeadersVisible = false;
            dgvScoreDetail.RowHeadersWidth = 40;

            // (1) 序号列 (不需要 DataPropertyName，由绘制事件生成)
            //var colIndex = new DataGridViewTextBoxColumn
            //{
            //    HeaderText = "序号",
            //    Width = 50,
            //    ReadOnly = true, // 序号不可编辑
            //    DefaultCellStyle = new DataGridViewCellStyle
            //    {
            //        Alignment = DataGridViewContentAlignment.MiddleCenter,
            //        BackColor = Color.LightGray
            //    }
            //};
            //dgvScoreDetail.Columns.Add(colIndex);

            // (2) 学号与姓名 (只读)
            // dgvScoreDetail.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "学号", DataPropertyName = "StudentNo", Width = 100, ReadOnly = true });
            // dgvScoreDetail.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "姓名", DataPropertyName = "StudentName", Width = 100, ReadOnly = true });

            // (3) 考勤 (下拉选择框)
            // var colAttendance = new DataGridViewComboBoxColumn
            // {
            //     HeaderText = "考勤",
            //     DataPropertyName = "Attendance", // 对应 StudentScoreRow 类的属性名
            //     Width = 100,
            //     FlatStyle = FlatStyle.Flat
            // };
            // colAttendance.Items.AddRange("到", "缺", "迟到", "早退");
            // dgvScoreDetail.Columns.Add(colAttendance);

            // (4) 纪律与质量 (数字输入)
            // dgvScoreDetail.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "纪律", DataPropertyName = "Discipline", Width = 80 });
            // dgvScoreDetail.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "质量", DataPropertyName = "Quality", Width = 80 });

            // (5) 绑定绘制序号的事件
            dgvScoreDetail.RowPostPaint += dgvScoreDetail_RowPostPaint;

            // 绑定数字输入校验事件 (纪律和质量限制为 0-100)
            dgvScoreDetail.CellValidating += dgvScoreDetail_CellValidating;

            // 绑定授课日期选择事件
            dtpTeachingDate.ValueChanged += DtpTeachingDate_ValueChanged;

            // 自动刷新课程下拉框事件绑定
            cmbCourseCode.SelectedIndexChanged += CmbCourseCode_SelectedIndexChanged;

            // 页面加载时主动触发一次，加载当天的课程
            LoadCoursesByDate(DateTime.Today);
        }

        // 1. 绘制第一列(序号)的逻辑
        private void dgvScoreDetail_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            string index = (e.RowIndex + 1).ToString();
            TextRenderer.DrawText(
                e.Graphics,
                index,
                dgvScoreDetail.DefaultCellStyle.Font,
                e.RowBounds,
                dgvScoreDetail.DefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right
            );
        }

        // 2. 校验输入是否为 0-100 的整数
        private void dgvScoreDetail_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // 跳过第一列(序号)和只读列
            if (e.ColumnIndex <= 2) return;

            // 获取用户输入的值并尝试转为整数
            if (int.TryParse(e.FormattedValue.ToString(), out int value))
            {
                // 如果超出 0-100 范围，取消离开当前单元格，并弹出警告
                if (value < 0 || value > 100)
                {
                    dgvScoreDetail.Rows[e.RowIndex].ErrorText = "请输入 0 到 100 之间的整数";
                    e.Cancel = true;
                }
            }
            else if (!string.IsNullOrEmpty(e.FormattedValue.ToString()))
            {
                // 输入的不是数字也不是空值时，弹出警告
                dgvScoreDetail.Rows[e.RowIndex].ErrorText = "请输入有效的数字 (0-100)";
                e.Cancel = true;
            }
        }

        // 3. 授课日期变更时，重新加载课程列表
        private void DtpTeachingDate_ValueChanged(object sender, EventArgs e)
        {
            // 先解绑事件，避免清空数据源时误触发刷新逻辑
            cmbCourseCode.SelectedIndexChanged -= CmbCourseCode_SelectedIndexChanged;

            cmbCourseCode.DataSource = null;
            dgvLogInfo.DataSource = null;
            dgvScoreDetail.DataSource = null;

            LoadCoursesByDate(dtpTeachingDate.Value.Date);

            cmbCourseCode.SelectedIndexChanged += CmbCourseCode_SelectedIndexChanged;
        }

        // 4. 根据日期加载课程列表
        /// <summary>
        /// 根据日期加载当天的课程列表到下拉框
        /// </summary>
        private void LoadCoursesByDate(DateTime date)
        {
            var list = new List<CourseItem>();
            string sql = $"SELECT {T_LogID}, {T_CourseCode}, {T_CourseName}, {T_TeachingHours}, " +
                         $"{T_ClassName}, {T_Classroom}, {T_Content} " +
                         $"FROM TeachingLogs WHERE date({T_Date}) = date(@Date) ORDER BY {T_TeachingHours}";

            using var conn = _db.CreateConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));


            // 诊断1：是否加载到了数据库文件
            // cmd.CommandText = "SELECT COUNT(*) FROM TeachingLogs";
            // var total = cmd.ExecuteScalar();
            // Debug.WriteLine($"TeachingLogs 总行数 = {total}");

            // 诊断2：直接看库里日期字段到底长什么样
            // cmd.CommandText = "SELECT DISTINCT TeachingDate FROM TeachingLogs LIMIT 10";
            // using var r = cmd.ExecuteReader();
            // while (r.Read()) Debug.WriteLine($"[{r.GetValue(0)}] 类型={r.GetValue(0).GetType().Name}");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CourseItem
                {
                    LogID = reader.GetInt32(reader.GetOrdinal(T_LogID)),
                    CourseCode = reader.GetString(reader.GetOrdinal(T_CourseCode)),
                    CourseName = reader.GetString(reader.GetOrdinal(T_CourseName)),
                    TeachingHours = reader.IsDBNull(reader.GetOrdinal(T_TeachingHours)) ? "" : reader.GetValue(reader.GetOrdinal(T_TeachingHours)).ToString(),
                    ClassName = reader.GetString(reader.GetOrdinal(T_ClassName)),
                    Classroom = reader.IsDBNull(reader.GetOrdinal(T_Classroom)) ? "" : reader.GetString(reader.GetOrdinal(T_Classroom)),
                    Content = reader.IsDBNull(reader.GetOrdinal(T_Content)) ? "" : reader.GetString(reader.GetOrdinal(T_Content)),
                });
            }

            if (list.Count == 0)
            {
                // 当天无课：放一个占位项，选它时双表会被清空
                cmbCourseCode.DataSource = new List<CourseItem> { new CourseItem { CourseCode = "-- 当天无课程 --" } };
                cmbCourseCode.DisplayMember = nameof(CourseItem.CourseCode);
                return;
            }

            cmbCourseCode.DataSource = list;
            cmbCourseCode.DisplayMember = nameof(CourseItem.CourseCode);
        }

        // 5. 当选择课程时，加载该课程的学生评分明细
        private void CmbCourseCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCourseCode.SelectedItem is not CourseItem item || item.LogID == 0)
            {
                dgvLogInfo.DataSource = null;
                dgvScoreDetail.DataSource = null;
                return;
            }

            RefreshLogInfoGrid(item);
            RefreshScoreGrid(item);
        }

        // 6. 刷新上方课程信息确认表
        /// <summary>
        /// 刷新上方课程信息确认表（只读展示 TeachingLog 详情）
        /// 上方课程信息确认表：把选中的这条 CourseItem 包成单元素列表绑定
        /// </summary>
        private void RefreshLogInfoGrid(CourseItem item)
        {
            dgvLogInfo.DataSource = new List<CourseItem> { item };
        }

        // 7. 刷新下方学生评分明细表
        /// <summary>
        /// 刷新下方学生评分明细表
        /// 下方学生评分明细表：按班级查学生
        /// </summary>
        private void RefreshScoreGrid(CourseItem item)
        {
            var rows = new List<StudentScoreRow>();
            string sql = $"SELECT {S_StudentID}, {S_StudentName}, {S_ClassName} " +
                         $"FROM Students WHERE {S_ClassName} = @ClassName ORDER BY {S_StudentID}";

            using var conn = _db.CreateConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@ClassName", item.ClassName);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new StudentScoreRow
                {
                    StudentID = reader.GetString(reader.GetOrdinal(S_StudentID)),
                    StudentName = reader.GetString(reader.GetOrdinal(S_StudentName)),
                    ClassName = reader.GetString(reader.GetOrdinal(S_ClassName)),
                    Rule1 = null,
                    Rule2 = null,
                    Rule3 = null
                });
            }

            dgvScoreDetail.DataSource = rows;
        }

    }
}
