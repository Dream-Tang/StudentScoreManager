using StudentScoreManager.Models;
using StudentScoreManager.Services;
using System.Data;

namespace StudentScoreManager.Forms
{
    public partial class Form1 : Form
    {
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

        // ==== 评分明细表列名（纵向长表：每人每维度一行）====
        private const string D_DetailID = "DetailID";
        private const string D_LogID = "LogID";
        private const string D_StudentID = "StudentID";
        private const string D_RuleName = "RuleName";
        private const string D_Score = "Score";

        // ==== 用户可编辑的"文本选项→分值"映射表（供考勤等文本型规则入库）====
        // 一张规则只要在 ScoreRuleOptions 里存在选项行，就在打分组里渲染成下拉；否则为数字输入列。
        private const string OPT_TABLE = "ScoreRuleOptions";

        // ==== 静态列（非规则列）名称常量 ====
        private const string COL_INDEX = "colIndex";   // 序号
        private const string COL_SID = "colSid";       // 学号
        private const string COL_SNAME = "colSName";   // 姓名

        /// <summary>规则列的元信息，挂在 DataGridViewColumn.Tag 上，用于保存/校验/回填时区分规则。</summary>
        private sealed class RuleColumnTag
        {
            public string RuleName = "";
            public double MaxScore = 100;
            public bool IsOption;                       // true=下拉(文本选项)；false=数字输入
            public Dictionary<string, double> OptionMap = new(); // 选项文本 -> 分值
        }

        public Form1()
        {
            InitializeComponent();
            _db = new DbHelper(AppConfig.DbPath); // 统一配置，全程序共用同一数据库路径
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializePage();
        }

        private void InitializePage()
        {
            // 确保"文本选项→分值"映射表存在（首次运行内置考勤默认映射，之后由用户在对话框里自行修改）
            EnsureOptionTable();

            // 上方【课程信息确认表】（只读，列已在 Designer 里设好 DataPropertyName）
            dgvLogInfo.EnableHeadersVisualStyles = false;
            dgvLogInfo.AutoGenerateColumns = false;
            dgvLogInfo.ReadOnly = true;
            dgvLogInfo.AllowUserToAddRows = false;
            dgvLogInfo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 下方【学生评分明细表】（可编辑；打分组列由 BuildScoreColumns 按 ScoringRules 动态生成）
            dgvScoreDetail.EnableHeadersVisualStyles = false;
            dgvScoreDetail.AutoGenerateColumns = false; // 关掉自动建列，改由代码按规则动态建
            dgvScoreDetail.AllowUserToAddRows = false;
            dgvScoreDetail.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvScoreDetail.RowHeadersVisible = false;

            // 下拉/单元格偶发的非法值不弹系统异常框（回填时若历史分值与选项对不上会触发）
            dgvScoreDetail.DataError += (s, ev) => { ev.ThrowException = false; };

            // 数字列输入校验（仅非选项列，按各规则 MaxScore 限制）
            dgvScoreDetail.CellValidating += dgvScoreDetail_CellValidating;

            // 事件绑定
            dtpTeachingDate.ValueChanged += DtpTeachingDate_ValueChanged;
            cmbCourseCode.SelectedIndexChanged += CmbCourseCode_SelectedIndexChanged;
            button1.Click += (s, e) => ReloadForCurrentCourse();            // 刷新
            button2.Click += ButtonSave_Click;                             // 保存数据
            button3.Click += (s, e) => FillAllPresent();                   // 全部出勤
            button4.Click += (s, e) => ClearSelectedRowsScores();          // 清除选定行
            button5.Click += (s, e) => OpenMappingEditor();                // 评分映射设置

            dtpTeachingDate.Value = DateTime.Today;
            LoadCoursesByDate(DateTime.Today);
        }

        // =====================================================================
        // 一、日期 / 课程下拉
        // =====================================================================
        private void DtpTeachingDate_ValueChanged(object sender, EventArgs e)
        {
            cmbCourseCode.SelectedIndexChanged -= CmbCourseCode_SelectedIndexChanged;
            cmbCourseCode.DataSource = null;
            dgvLogInfo.DataSource = null;
            dgvScoreDetail.DataSource = null;
            LoadCoursesByDate(dtpTeachingDate.Value.Date);
            cmbCourseCode.SelectedIndexChanged += CmbCourseCode_SelectedIndexChanged;
        }

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
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CourseItem
                {
                    LogID = reader.GetInt32(reader.GetOrdinal(T_LogID)),
                    CourseCode = reader.IsDBNull(reader.GetOrdinal(T_CourseCode)) ? "" : reader.GetString(reader.GetOrdinal(T_CourseCode)),
                    CourseName = reader.GetString(reader.GetOrdinal(T_CourseName)),
                    TeachingHours = reader.IsDBNull(reader.GetOrdinal(T_TeachingHours)) ? "" : reader.GetValue(reader.GetOrdinal(T_TeachingHours)).ToString(),
                    ClassName = reader.GetString(reader.GetOrdinal(T_ClassName)),
                    Classroom = reader.IsDBNull(reader.GetOrdinal(T_Classroom)) ? "" : reader.GetString(reader.GetOrdinal(T_Classroom)),
                    Content = reader.IsDBNull(reader.GetOrdinal(T_Content)) ? "" : reader.GetString(reader.GetOrdinal(T_Content)),
                });
            }

            if (list.Count == 0)
            {
                cmbCourseCode.DataSource = new List<CourseItem> { new CourseItem { CourseCode = "-- 当天无课程 --" } };
                cmbCourseCode.DisplayMember = nameof(CourseItem.CourseCode);
                return;
            }

            cmbCourseCode.DataSource = list;
            cmbCourseCode.DisplayMember = nameof(CourseItem.CourseCode);
        }

        private void CmbCourseCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCourseCode.SelectedItem is not CourseItem item || item.LogID == 0)
            {
                dgvLogInfo.DataSource = null;
                dgvScoreDetail.DataSource = null;
                if (dgvScoreDetail.Columns.Count > 0) dgvScoreDetail.Rows.Clear();
                return;
            }
            RefreshLogInfoGrid(item);
            RefreshScoreGrid(item);
        }

        private void RefreshLogInfoGrid(CourseItem item)
        {
            dgvLogInfo.DataSource = new List<CourseItem> { item };
        }

        // =====================================================================
        // 二、动态生成打分组列 + 回填
        // =====================================================================
        /// <summary>按 ScoringRules 动态生成打分组列（列标题=RuleName），有选项的规则渲染为下拉，否则数字列。</summary>
        private void BuildScoreColumns()
        {
            dgvScoreDetail.Columns.Clear();

            // 静态列：序号 / 学号 / 姓名（Tag 保持 null，用于在校验/保存时与规则列区分）
            dgvScoreDetail.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = COL_INDEX,
                HeaderText = "序号",
                Width = 60,
                ReadOnly = true,
                Tag = null,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            dgvScoreDetail.Columns.Add(new DataGridViewTextBoxColumn
            { Name = COL_SID, HeaderText = "学号", Width = 140, ReadOnly = true, Tag = null });
            dgvScoreDetail.Columns.Add(new DataGridViewTextBoxColumn
            { Name = COL_SNAME, HeaderText = "姓名", Width = 100, ReadOnly = true, Tag = null });

            // 读评分规则（按 SortOrder 排序），逐条生成一列
            var rules = LoadScoringRules();
            foreach (var r in rules)
            {
                var opts = LoadRuleOptions(r.RuleName);
                var tag = new RuleColumnTag { RuleName = r.RuleName, MaxScore = r.MaxScore };

                if (opts.Count > 0)
                {
                    // 文本选项型（如考勤）：下拉列
                    tag.IsOption = true;
                    tag.OptionMap = opts;
                    var combo = new DataGridViewComboBoxColumn
                    {
                        HeaderText = r.RuleName,          // RuleName 直接作为列标题 -> 与入库 RuleName 一致
                        Name = "rule_" + Guid.NewGuid().ToString("N"),
                        Width = 110,
                        FlatStyle = FlatStyle.Flat,
                        Tag = tag
                    };
                    foreach (var key in opts.Keys) combo.Items.Add(key);
                    dgvScoreDetail.Columns.Add(combo);
                }
                else
                {
                    // 数值型
                    tag.IsOption = false;
                    dgvScoreDetail.Columns.Add(new DataGridViewTextBoxColumn
                    {
                        HeaderText = r.RuleName,          // RuleName 直接作为列标题
                        Name = "rule_" + Guid.NewGuid().ToString("N"),
                        Width = 110,
                        Tag = tag
                    });
                }
            }
        }

        private struct RuleInfo { public string RuleName; public double MaxScore; }

        private List<RuleInfo> LoadScoringRules()
        {
            var list = new List<RuleInfo>();
            string sql = "SELECT RuleName, MaxScore FROM ScoringRules ORDER BY SortOrder, RuleName";
            using var conn = _db.CreateConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new RuleInfo
                {
                    RuleName = reader.GetString(0),
                    MaxScore = reader.IsDBNull(1) ? 100 : reader.GetDouble(1)
                });
            }
            return list;
        }

        /// <summary>刷新下方学生评分明细表：建列 -> 按班级拉学生 -> 回填已存分数。</summary>
        private void RefreshScoreGrid(CourseItem item)
        {
            BuildScoreColumns();

            var students = new List<(string Sid, string Name)>();
            string sql = $"SELECT {S_StudentID}, {S_StudentName} FROM Students " +
                         $"WHERE {S_ClassName} = @ClassName ORDER BY {S_StudentID}";
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@ClassName", item.ClassName);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    students.Add((reader.GetString(reader.GetOrdinal(S_StudentID)),
                                  reader.GetString(reader.GetOrdinal(S_StudentName))));
                }
            }

            dgvScoreDetail.Rows.Clear();
            int idx = 1;
            foreach (var st in students)
            {
                int r = dgvScoreDetail.Rows.Add();
                var row = dgvScoreDetail.Rows[r];
                row.Cells[COL_INDEX].Value = idx++;
                row.Cells[COL_SID].Value = st.Sid;
                row.Cells[COL_SNAME].Value = st.Name;
            }

            BackfillSavedScores(item.LogID);
        }

        /// <summary>回填该节课已保存过的分数：ScoreDetails 已有记录则填回表格（下拉按分值反查文本）。</summary>
        private void BackfillSavedScores(int logId)
        {
            // (StudentID, RuleName) -> Score
            var saved = new Dictionary<(string, string), double>();
            string sql = $"SELECT {D_StudentID}, {D_RuleName}, {D_Score} FROM ScoreDetails WHERE {D_LogID} = @LogID";
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@LogID", logId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string sid = reader.GetString(0);
                    string rule = reader.GetString(1);
                    double score = reader.IsDBNull(2) ? 0 : reader.GetDouble(2);
                    saved[(sid, rule)] = score;
                }
            }

            foreach (DataGridViewRow row in dgvScoreDetail.Rows)
            {
                string sid = row.Cells[COL_SID].Value?.ToString();
                if (string.IsNullOrEmpty(sid)) continue;

                foreach (DataGridViewColumn col in dgvScoreDetail.Columns)
                {
                    if (col.Tag is not RuleColumnTag tag) continue;
                    if (!saved.TryGetValue((sid, tag.RuleName), out double score)) continue;

                    if (tag.IsOption)
                    {
                        // 反查：分值对应的选项文本（分值相同取第一个）
                        string? text = tag.OptionMap.FirstOrDefault(kv => Math.Abs(kv.Value - score) < 1e-9).Key;
                        if (text != null && ((DataGridViewComboBoxColumn)col).Items.Contains(text))
                            row.Cells[col.Name].Value = text;
                    }
                    else
                    {
                        row.Cells[col.Name].Value = score.ToString();
                    }
                }
            }
        }

        // =====================================================================
        // 三、输入校验（仅数值列）
        // =====================================================================
        private void dgvScoreDetail_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var col = dgvScoreDetail.Columns[e.ColumnIndex];
            if (col.Tag is not RuleColumnTag tag || tag.IsOption) return; // 序号/学号/姓名/下拉列跳过

            string input = e.FormattedValue?.ToString() ?? "";
            if (string.IsNullOrEmpty(input)) return; // 允许留空（表示不评该维度）

            if (double.TryParse(input, out double v) && v >= 0 && v <= tag.MaxScore) return;

            dgvScoreDetail.Rows[e.RowIndex].ErrorText = $"请输入 0 到 {tag.MaxScore:0.##} 之间的数字";
            e.Cancel = true;
        }

        // =====================================================================
        // 四、四个按钮动作
        // =====================================================================
        /// <summary>刷新：重新按当前课程建列 + 拉学生 + 回填。</summary>
        private void ReloadForCurrentCourse()
        {
            if (cmbCourseCode.SelectedItem is CourseItem item && item.LogID != 0)
                RefreshScoreGrid(item);
            else
                LoadCoursesByDate(dtpTeachingDate.Value.Date);
        }

        /// <summary>全部出勤：把所有"文本选项"列填成该规则里分值最高的选项（通常为"到"）。</summary>
        private void FillAllPresent()
        {
            var presentCols = new List<DataGridViewColumn>();
            var presentVals = new List<string>();
            foreach (DataGridViewColumn col in dgvScoreDetail.Columns)
            {
                if (col.Tag is RuleColumnTag tag && tag.IsOption && tag.OptionMap.Count > 0)
                {
                    var best = tag.OptionMap.OrderByDescending(kv => kv.Value).First().Key;
                    presentCols.Add(col); presentVals.Add(best);
                }
            }
            if (presentCols.Count == 0)
            {
                MessageBox.Show("当前没有文本选项型规则（如考勤），无需执行全部出勤。", "提示");
                return;
            }
            foreach (DataGridViewRow row in dgvScoreDetail.Rows)
            {
                if (row.IsNewRow) continue;
                for (int i = 0; i < presentCols.Count; i++)
                    row.Cells[presentCols[i].Name].Value = presentVals[i];
            }
        }

        /// <summary>清除选定行：清空选中行的打分组内容（保留学生行本身）。</summary>
        private void ClearSelectedRowsScores()
        {
            var ruleCols = dgvScoreDetail.Columns.Cast<DataGridViewColumn>().Where(c => c.Tag is RuleColumnTag).ToList();
            var rows = dgvScoreDetail.SelectedRows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).ToList();
            if (rows.Count == 0)
            {
                MessageBox.Show("请先选中要清除评分的行。", "提示");
                return;
            }
            foreach (var row in rows)
                foreach (var col in ruleCols)
                    row.Cells[col.Name].Value = null;
        }

        /// <summary>保存数据：逐行逐规则拆分写入 ScoreDetails；存在(LogID,StudentID,RuleName)则更新，否则插入。单事务，出错整体回滚。</summary>
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            if (cmbCourseCode.SelectedItem is not CourseItem item || item.LogID == 0)
            {
                MessageBox.Show("请先选择一节有效课程。", "提示");
                return;
            }
            dgvScoreDetail.EndEdit(); // 确保正在编辑的单元格先提交

            int logId = item.LogID;
            var errors = new List<string>();
            int affected = 0;

            using var conn = _db.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                foreach (DataGridViewRow row in dgvScoreDetail.Rows)
                {
                    if (row.IsNewRow) continue;
                    string sid = row.Cells[COL_SID].Value?.ToString() ?? "";
                    string name = row.Cells[COL_SNAME].Value?.ToString() ?? "";

                    foreach (DataGridViewColumn col in dgvScoreDetail.Columns)
                    {
                        if (col.Tag is not RuleColumnTag tag) continue;
                        object? raw = row.Cells[col.Name].Value;
                        string text = raw?.ToString() ?? "";
                        if (string.IsNullOrWhiteSpace(text)) continue; // 空 = 该维度不评，跳过

                        double score;
                        if (tag.IsOption)
                        {
                            if (!tag.OptionMap.TryGetValue(text, out score))
                            {
                                errors.Add($"学生[{name}] 规则[{tag.RuleName}] 选项[{text}] 未配置分值");
                                continue;
                            }
                        }
                        else
                        {
                            if (!double.TryParse(text, out score) || score < 0 || score > tag.MaxScore)
                            {
                                errors.Add($"学生[{name}] 规则[{tag.RuleName}] 分值[{text}] 非法(0~{tag.MaxScore:0.##})");
                                continue;
                            }
                        }

                        // 存在即更新（业务键：LogID + StudentID + RuleName）
                        int changed = _db.ExecuteNonQuery(
                            $"UPDATE ScoreDetails SET {D_Score} = @Score WHERE {D_LogID} = @LogID AND {D_StudentID} = @Sid AND {D_RuleName} = @Rule",
                            new { Score = score, LogID = logId, Sid = sid, Rule = tag.RuleName }, conn, trans);

                        if (changed == 0)
                        {
                            _db.ExecuteNonQuery(
                                $"INSERT INTO ScoreDetails ({D_LogID}, {D_StudentID}, {D_RuleName}, {D_Score}) VALUES (@LogID, @Sid, @Rule, @Score)",
                                new { LogID = logId, Sid = sid, Rule = tag.RuleName, Score = score }, conn, trans);
                        }
                        affected++;
                    }
                }

                if (errors.Count > 0)
                {
                    trans.Rollback();
                    MessageBox.Show($"保存失败，已回滚（未写入任何数据）。\n\n【问题】\n{string.Join("\n", errors)}",
                        "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                trans.Commit();
                MessageBox.Show($"保存成功，本次写入/更新 {affected} 条评分。", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                try { trans.Rollback(); } catch { }
                MessageBox.Show("保存失败（数据库异常）：" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =====================================================================
        // 五、文本选项→分值 映射（建表/默认种子 + 用户可编辑对话框）
        // =====================================================================
        private void EnsureOptionTable()
        {
            _db.ExecuteNonQuery(
                $"CREATE TABLE IF NOT EXISTS {OPT_TABLE} (" +
                "RuleName TEXT NOT NULL, OptionText TEXT NOT NULL, ScoreValue REAL NOT NULL, " +
                $"PRIMARY KEY (RuleName, OptionText))");

            // 首次运行（映射表为空）时，若存在名为"考勤"的规则，写入一套默认可改映射
            int cnt = _db.ExecuteScalar<int>($"SELECT COUNT(*) FROM {OPT_TABLE}");
            if (cnt == 0)
            {
                int hasAttend = _db.ExecuteScalar<int>("SELECT COUNT(*) FROM ScoringRules WHERE RuleName = '考勤'");
                if (hasAttend > 0)
                {
                    var seed = new (string, double)[]
                    {
                        ("到", 100), ("迟到", 60), ("请假", 0), ("公假", 100), ("早退", 50), ("缺勤", 0)
                    };
                    foreach (var (txt, sc) in seed)
                    {
                        _db.ExecuteNonQuery(
                            $"INSERT OR IGNORE INTO {OPT_TABLE} (RuleName, OptionText, ScoreValue) VALUES ('考勤', @T, @S)",
                            new { T = txt, S = sc });
                    }
                }
            }
        }

        private Dictionary<string, double> LoadRuleOptions(string ruleName)
        {
            var map = new Dictionary<string, double>();
            string sql = $"SELECT OptionText, ScoreValue FROM {OPT_TABLE} WHERE RuleName = @Rule ORDER BY ScoreValue DESC";
            using var conn = _db.CreateConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@Rule", ruleName);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(1)) map[reader.GetString(0)] = reader.GetDouble(1);
            }
            return map;
        }

        /// <summary>用户自行编辑"文本选项→分值"映射：选规则 -> 编辑其(选项文本, 分值) -> 保存覆盖该规则全部选项。</summary>
        private void OpenMappingEditor()
        {
            var rules = LoadScoringRules();
            if (rules.Count == 0)
            {
                MessageBox.Show("评分规则表(ScoringRules)为空，请先维护评分规则。", "提示");
                return;
            }

            using var dlg = new Form
            {
                Text = "评分映射设置（文本选项 → 分值）",
                ClientSize = new Size(420, 380),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable
            };

            var lblRule = new Label { Text = "选择规则：", Left = 12, Top = 15, AutoSize = true };
            var cmbRule = new ComboBox { Left = 100, Top = 11, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var r in rules) cmbRule.Items.Add(r.RuleName);

            var grid = new DataGridView
            {
                Left = 12,
                Top = 48,
                Width = 396,
                Height = 250,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoGenerateColumns = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "选项文本", Name = "cText", Width = 200 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "分值", Name = "cScore", Width = 180 });

            void LoadRuleToGrid()
            {
                grid.Rows.Clear();
                if (cmbRule.SelectedItem == null) return;
                foreach (var kv in LoadRuleOptions(cmbRule.SelectedItem.ToString()!))
                    grid.Rows.Add(kv.Key, kv.Value);
            }
            cmbRule.SelectedIndexChanged += (s, e) => LoadRuleToGrid();

            var btnDel = new Button { Text = "删除选中行", Left = 12, Top = 310, Width = 110, Anchor = AnchorStyles.Left | AnchorStyles.Bottom };
            btnDel.Click += (s, e) =>
            {
                var sel = grid.SelectedRows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).ToList();
                foreach (var r in sel) grid.Rows.Remove(r);
            };

            var btnOk = new Button { Text = "保存", Left = 200, Top = 310, Width = 90, Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
            var btnCancel = new Button { Text = "关闭", Left = 306, Top = 310, Width = 90, Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
            btnCancel.Click += (s, e) => dlg.Close();

            btnOk.Click += (s, e) =>
            {
                if (cmbRule.SelectedItem == null) { MessageBox.Show("请先选择规则"); return; }
                string rule = cmbRule.SelectedItem.ToString()!;

                var rows = grid.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow)
                    .Select(r => new
                    {
                        Text = r.Cells["cText"].Value?.ToString(),
                        ScoreStr = r.Cells["cScore"].Value?.ToString()
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                    .ToList();

                // 校验分值合法性 + 去重
                var parsed = new List<(string, double)>();
                foreach (var x in rows)
                {
                    if (!double.TryParse(x.ScoreStr, out double sc))
                    {
                        MessageBox.Show($"选项[{x.Text}] 的分值[{x.ScoreStr}]不是有效数字。", "校验失败");
                        return;
                    }
                    if (parsed.Any(p => p.Item1 == x.Text))
                    {
                        MessageBox.Show($"选项文本[{x.Text}] 重复，请合并。", "校验失败");
                        return;
                    }
                    parsed.Add((x.Text!, sc));
                }

                // 覆盖该规则的全部选项（单事务）
                using var conn = _db.CreateConnection();
                conn.Open();
                using var trans = conn.BeginTransaction();
                try
                {
                    _db.ExecuteNonQuery($"DELETE FROM {OPT_TABLE} WHERE RuleName = @Rule", new { Rule = rule }, conn, trans);
                    foreach (var (txt, sc) in parsed)
                        _db.ExecuteNonQuery(
                            $"INSERT INTO {OPT_TABLE} (RuleName, OptionText, ScoreValue) VALUES (@Rule, @T, @S)",
                            new { Rule = rule, T = txt, S = sc }, conn, trans);
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("保存映射失败：" + ex.Message, "错误");
                    return;
                }

                // 映射变化后，重建当前打分组列（下拉选项即时更新），并回填
                if (cmbCourseCode.SelectedItem is CourseItem cur && cur.LogID != 0)
                    RefreshScoreGrid(cur);

                MessageBox.Show($"规则[{rule}] 的映射已保存。", "成功");
                LoadRuleToGrid();
            };

            dlg.Controls.AddRange(new Control[] { lblRule, cmbRule, grid, btnDel, btnOk, btnCancel });
            if (cmbRule.Items.Count > 0) cmbRule.SelectedIndex = 0;
            LoadRuleToGrid();
            dlg.ShowDialog(this);
        }
    }
}