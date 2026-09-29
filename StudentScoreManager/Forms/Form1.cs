

using StudentScoreManager.Models;
using StudentScoreManager.Services;
using System.Data;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using Krypton.Toolkit;

namespace StudentScoreManager.Forms
{
    public partial class Form1 : KryptonForm
    {
        private readonly DbHelper _db;

        // ==== 当前选中的课程（由左侧课程卡片点击设定）====
        private CourseItem? _currentCourse = null;

        // ==== 左侧课程卡片 ====
        private CourseCard? _currentCard = null;

        // ==== 未保存修改标记 ====
        private bool _isDirty = false;

        // 程序化加载评分表时置 true，避免回填/建行被误判为"用户改动"
        private bool _loading = false;

        // 用户在"未保存"拦截里选"取消"时需要把日期控件回退，回退时不能再触发一次切换逻辑
        private bool _suppressDateEvent = false;
        private DateTime _lastDate = DateTime.Today;

        // TeachingLogs 实际拥有的列（用于兼容"备注"列存在与否，避免 SQL 报错）
        private HashSet<string> _logColumns = new(StringComparer.OrdinalIgnoreCase);

        // ==== 每天 10 节课，两节一个卡片 ====
        private static readonly string[] SectionLabels = { "一、二", "三、四", "五、六", "七、八", "九、十" };

        // ==== TeachingLogs 表列名 ====
        private const string T_LogID = "LogID";
        private const string T_Date = "TeachingDate";
        private const string T_CourseCode = "CourseCode";
        private const string T_CourseName = "CourseName";
        private const string T_TeachingHours = "TeachingHours";
        private const string T_ClassName = "ClassName";
        private const string T_Classroom = "Classroom";
        private const string T_Content = "TeachingContent";
        private const string T_Remark = "Remark";

        // ==== Students 表列名 ====
        private const string S_StudentID = "StudentID";
        private const string S_StudentName = "StudentName";
        private const string S_ClassName = "ClassName";

        // ==== 评分明细表列名（纵向长表：每人每维度一行）====
        private const string D_LogID = "LogID";
        private const string D_StudentID = "StudentID";
        private const string D_RuleName = "RuleName";
        private const string D_Score = "Score";

        // ==== 用户可编辑的"文本选项→分值"映射表（供考勤等文本型规则入库）====
        private const string OPT_TABLE = "ScoreRuleOptions";

        // ==== 静态列（非规则列）名称常量 ====
        private const string COL_INDEX = "colIndex";
        private const string COL_SID = "colSid";
        private const string COL_SNAME = "colSName";

        // ==== 主题色（与 Krypton 浅色主题搭配的中性色 + 强调蓝）====
        private static readonly Color Accent = Color.FromArgb(0, 120, 215);
        private static readonly Color CardBack = Color.White;
        private static readonly Color CardBackSelected = Color.FromArgb(232, 243, 253);
        private static readonly Color CardBackEmpty = Color.FromArgb(246, 247, 249);
        private static readonly Color CardBorder = Color.FromArgb(224, 227, 231);
        private static readonly Color CardBorderHover = Color.FromArgb(160, 200, 235);
        private static readonly Color TextMain = Color.FromArgb(40, 44, 50);
        private static readonly Color TextMuted = Color.FromArgb(120, 126, 135);
        private static readonly Color TextDisabled = Color.FromArgb(175, 178, 182);

        /// <summary>规则列的元信息，挂在 DataGridViewColumn.Tag 上，用于保存/校验/回填时区分规则。</summary>
        private sealed class RuleColumnTag // sealed类不可继承，避免外部误用
        {
            public string RuleName = "";
            public double MaxScore = 100;
            public bool IsOption;                           // true=下拉(文本选项)；false=数字输入
            public Dictionary<string, double> OptionMap = new(); // 选项文本 -> 分值
        }

        /// <summary>一个卡片槽位 = 一个节次。Item 为 null 表示该节次当天无课；Remark 仅用于卡片展示，不进数据库。</summary>
        private sealed class CourseSlot
        {
            public string Section = "";
            public CourseItem? Item;
            public string Remark = "";
        }

        // ==== 双缓冲 FlowLayoutPanel：消除卡片切换/滚动时的闪烁 ====
        private sealed class DoubleBufferedFlowLayoutPanel : FlowLayoutPanel
        {
            public DoubleBufferedFlowLayoutPanel()
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            }
        }

        // ==== 异步加载：评分规则定义 + 一次刷表所需的数据快照 ====
        private sealed class RuleDef
        {
            public string RuleName = "";
            public double MaxScore = 100;
            public Dictionary<string, double> OptionMap = new();
        }
        private sealed class GridData
        {
            public List<RuleDef> Rules = new();
            public List<(string Sid, string Name)> Students = new();
            public Dictionary<(string, string), double> Saved = new();
        }

        // 异步刷表并发令牌：只应用最后一次切换结果，丢弃过期回调
        private int _loadToken;

        /// <summary>卡片只创建一次（5 张固定节次），之后一律 SetCard 复用，避免重建触发重排。</summary>
        private void EnsureCardsBuilt()
        {
            if (_cards.Count > 0) return;
            for (int i = 0; i < SectionLabels.Length; i++)
            {
                var card = new CourseCard();
                card.Click += Card_Click;
                _cards.Add(card);
                flpCourses.Controls.Add(card);
            }
        }

        public Form1()
        {
            InitializeComponent();
            _db = new DbHelper(AppConfig.DbPath); // 统一配置，全程序共用同一数据库路径
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            InitializePage();
        }

        private void InitializePage()
        {
            // 确保"文本选项→分值"映射表存在（首次运行内置考勤默认映射，之后由用户在对话框里自行修改）
            EnsureOptionTable();
            _logColumns = ReadTeachingLogColumns();

            // ===== 左侧：卡片列表容器 =====
            flpCourses.BackColor = Color.Transparent;
            flpCourses.SizeChanged += (s, e) => LayoutCards();   // 左侧面板拖宽/滚动条出现时跟随重排

            // ===== 右侧：评分工作台（现代扁平风格）=====
            StyleScoreGrid();

            // ===== Excel 式快速填写：校验与编辑手感 =====
            dgvScoreDetail.CellValidating += dgvScoreDetail_CellValidating;
            dgvScoreDetail.EditingControlShowing += DgvScoreDetail_EditingControlShowing;

            // ===== 未保存修改跟踪 =====
            dgvScoreDetail.CellValueChanged += DgvScoreDetail_CellValueChanged;
            // 下拉/复选一类控件改值后立即提交，保证 CellValueChanged 及时触发
            dgvScoreDetail.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvScoreDetail.IsCurrentCellDirty) dgvScoreDetail.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            FormClosing += Form1_FormClosing;

            // ===== 事件绑定（控件已在设计器声明，这里只做接线，符合设计器惯例）=====
            dtpDate.ValueChanged += DtpDate_ValueChanged;
            btnRefresh.Click += (s, e) => ReloadForCurrentCourse();
            btnMappingEditor.Click += (s, e) => OpenMappingEditor();
            btnPresent.Click += (s, e) => FillAllPresent();
            btnClear.Click += (s, e) => ClearSelectedRowsScores();
            btnSave.Click += ButtonSave_Click;

            _lastDate = DateTime.Today;
            // DateTimePicker 默认值本就是今天，赋值不一定触发 ValueChanged -> 显式建一次卡片
            _suppressDateEvent = true;
            dtpDate.Value = _lastDate;
            _suppressDateEvent = false;
            BuildCourseCards(_lastDate);
            SetSaveState(false);
        }

        /// <summary>评分表扁平化样式：无外框、浅灰分隔线、行高加大、表头去渐变。</summary>
        private void StyleScoreGrid()
        {
            dgvScoreDetail.EnableHeadersVisualStyles = false;
            dgvScoreDetail.AutoGenerateColumns = false;      // 关掉自动建列，改由代码按规则动态建
            dgvScoreDetail.AllowUserToAddRows = false;
            dgvScoreDetail.RowHeadersVisible = false;
            dgvScoreDetail.BorderStyle = BorderStyle.None;
            dgvScoreDetail.BackgroundColor = Color.White;
            dgvScoreDetail.GridColor = Color.FromArgb(233, 235, 238);
            dgvScoreDetail.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvScoreDetail.AllowUserToResizeRows = false;

            // Excel 式快速填写的选区前提：CellSelect + MultiSelect
            dgvScoreDetail.MultiSelect = true;
            dgvScoreDetail.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvScoreDetail.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvScoreDetail.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

            dgvScoreDetail.RowTemplate.Height = 34;
            dgvScoreDetail.ColumnHeadersHeight = 40;
            dgvScoreDetail.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            var bodyFont = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            var headFont = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point);

            dgvScoreDetail.DefaultCellStyle.Font = bodyFont;
            dgvScoreDetail.DefaultCellStyle.SelectionBackColor = Color.FromArgb(205, 226, 247);
            dgvScoreDetail.DefaultCellStyle.SelectionForeColor = TextMain;
            dgvScoreDetail.ColumnHeadersDefaultCellStyle.Font = headFont;
            dgvScoreDetail.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 251);
            dgvScoreDetail.ColumnHeadersDefaultCellStyle.ForeColor = TextMain;
            dgvScoreDetail.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 249, 251);
            dgvScoreDetail.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 253);
            dgvScoreDetail.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(205, 226, 247);

            // 下拉/单元格偶发的非法值不弹系统异常框（回填时若历史分值与选项对不上会触发）
            dgvScoreDetail.DataError += (s, ev) => { ev.ThrowException = false; };

            lblCourseTitle.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblCourseTitle.ForeColor = TextMain;

            // 开启 DataGridView 内部双缓冲属性，消除切卡片/填表时的整表闪烁
            typeof(DataGridView)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(dgvScoreDetail, true);
        }

        // =====================================================================
        // 一、日期 -> 课程卡片 -> 点击选课
        // =====================================================================

        /// <summary>判断一条 CourseItem 是否为"有课"的有效槽位（LogID 必须存在且非 0）。</summary>
        private static bool HasCourse(CourseItem? item)
            => item != null && item.LogID.HasValue && item.LogID.Value != 0;

        /// <summary>读取 TeachingLogs 的真实列名，用于兼容"备注"列不存在的情况（不存在的列用 '' 占位，SQL 不会报错）。</summary>
        private HashSet<string> ReadTeachingLogColumns()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var conn = _db.CreateConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA table_info(TeachingLogs)";
                using var reader = cmd.ExecuteReader();
                int ordName = reader.GetOrdinal("name");
                while (reader.Read()) set.Add(reader.GetString(ordName));
            }
            catch { /* 读不到就按"无备注列"处理 */ }
            return set;
        }

        /// <summary>列存在则取原名，不存在则用常量列占位，保证后续 GetOrdinal 一定成功。</summary>
        private string LogCol(string col)
            => _logColumns.Contains(col) ? col : $"'' AS {col}";

        private void DtpDate_ValueChanged(object? sender, EventArgs e)
        {
            if (_suppressDateEvent) return;

            // 切换日期同样要先处理未保存修改；取消则把日期回退
            if (!ConfirmLeaveCourse())
            {
                _suppressDateEvent = true;
                dtpDate.Value = _lastDate;
                _suppressDateEvent = false;
                return;
            }

            _lastDate = dtpDate.Value.Date;
            _currentCourse = null;
            _currentCard = null;
            dgvScoreDetail.DataSource = null;
            if (dgvScoreDetail.Columns.Count > 0) dgvScoreDetail.Rows.Clear();
            lblCourseTitle.Text = "请选择左侧课程";
            SetSaveState(false);
            BuildCourseCards(_lastDate);
        }

        /// <summary>按日期查课，生成固定 5 个节次卡片（有课填内容，无课置灰占位），并自动选中第一个有课卡片。</summary>
        private void BuildCourseCards(DateTime date)
        {
            var byHours = QueryCoursesByDate(date);   // 切日期本就要查；高频的"切卡片"走异步不重建
            EnsureCardsBuilt();                        // 卡片只建一次，之后 SetCard 复用
            flpCourses.SuspendLayout();
            for (int i = 0; i < _cards.Count; i++)
            {
                string label = SectionLabels[i];
                byHours.TryGetValue(label, out var item);
                _cards[i].SetCard(item, label, item?.Remark ?? "");
            }
            flpCourses.ResumeLayout(true);
            LayoutCards();

            _currentCard = null;
            var first = _cards.FirstOrDefault(c => HasCourse(c.Course));
            if (first != null) ActivateCard(first);
            else SetSaveState(false);
        }

        /// <summary>查当天课程，按 TeachingHours 归一后建立 节次 -> 课程 映射（同节次取第一条）。</summary>
        private Dictionary<string, CourseItem> QueryCoursesByDate(DateTime date)
        {
            var byHours = new Dictionary<string, CourseItem>();
            string sql = $"SELECT {T_LogID}, {LogCol(T_CourseCode)} AS {T_CourseCode}, " +
                         $"{LogCol(T_CourseName)} AS {T_CourseName}, {LogCol(T_TeachingHours)} AS {T_TeachingHours}, " +
                         $"{LogCol(T_ClassName)} AS {T_ClassName}, {LogCol(T_Classroom)} AS {T_Classroom}, " +
                         $"{LogCol(T_Content)} AS {T_Content}, {LogCol(T_Remark)} AS {T_Remark} " +
                         $"FROM TeachingLogs WHERE date({T_Date}) = date(@Date) ORDER BY {T_TeachingHours}";

            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var item = new CourseItem
                    {
                        LogID = reader.GetInt32(reader.GetOrdinal(T_LogID)),
                        CourseCode = GetText(reader, T_CourseCode),
                        CourseName = GetText(reader, T_CourseName),
                        TeachingHours = GetText(reader, T_TeachingHours),
                        ClassName = GetText(reader, T_ClassName),
                        Classroom = GetText(reader, T_Classroom),
                        Content = GetText(reader, T_Content),
                        Remark = GetText(reader, T_Remark),
                    };
                    string key = (item.TeachingHours ?? "").Trim();
                    if (!string.IsNullOrEmpty(key) && !byHours.ContainsKey(key))
                        byHours[key] = item;
                }
            }
            return byHours;
        }

        private static string GetText(IDataReader reader, string col)
        {
            int o = reader.GetOrdinal(col);
            return reader.IsDBNull(o) ? "" : (reader.GetValue(o)?.ToString() ?? "");
        }

        /// <summary>卡片宽度跟随左侧面板宽度自适应（减去滚动条与内边距）。</summary>
        private void LayoutCards()
        {
            int w = flpCourses.ClientSize.Width - flpCourses.Padding.Horizontal - 4;
            if (w < 200) w = 200;
            flpCourses.SuspendLayout();
            foreach (var card in _cards)
            {
                if (card.Width != w) card.Width = w;   // 宽度变化会触发卡片重绘圆角边框
            }
            flpCourses.ResumeLayout(false);           // 不强制立即重排，避免逐个改宽触发连环布局
        }

        private void Card_Click(object? sender, EventArgs e)
        {
            if (sender is not CourseCard card) return;
            if (!HasCourse(card.Course)) return;    // 无课卡片不响应
            if (card == _currentCard) return;

            if (!ConfirmLeaveCourse()) return;         // 有未保存内容先问清
            ActivateCard(card);
        }

        /// <summary>真正执行切换：高亮卡片、刷新右侧评分表、重置未保存标记。</summary>
        private void ActivateCard(CourseCard card)
        {
            if (_currentCard != null && _currentCard != card) _currentCard.Selected = false;
            card.Selected = true;
            _currentCard = card;
            _currentCourse = card.Course;

            lblCourseTitle.Text = $"{_currentCourse!.CourseName}  ·  {_currentCourse.ClassName}  ·  第{card.Section}节";
            SetSaveState(false);
            RefreshScoreGridAsync(_currentCourse);
        }

        /// <summary>离开当前课程前的未保存拦截。返回 false 表示用户取消，调用方应放弃切换。</summary>
        private bool ConfirmLeaveCourse()
        {
            if (!_isDirty) return true;

            var r = MessageBox.Show(
                "当前评分表有尚未保存的修改。",
                "未保存的修改",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1);

            if (r == DialogResult.Cancel) return false;   // 留在当前课程
            if (r == DialogResult.No) return true;        // 放弃修改

            SaveCurrentCourse();                          // 先保存；保存内部失败会自行提示
            return !_isDirty;                             // 保存失败（仍为脏）则不切换
        }

        private void SetSaveState(bool dirty)
        {
            _isDirty = dirty;
            if (dirty)
            {
                lblSaveState.Text = "● 有未保存修改";
                lblSaveState.ForeColor = Color.FromArgb(200, 120, 0);
            }
            else
            {
                lblSaveState.Text = "● 已保存";
                lblSaveState.ForeColor = Color.FromArgb(0, 150, 90);
            }
        }

        private void DgvScoreDetail_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (_loading) return;
            if (e.RowIndex < 0) return;
            if (!_isDirty) SetSaveState(true);
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!ConfirmLeaveCourse()) e.Cancel = true;
        }

        // =====================================================================
        // 二、动态生成打分组列 + 回填
        // =====================================================================

        /// <summary>按 ScoringRules 动态生成打分组列（列标题=RuleName），有选项的规则渲染为下拉，否则数字列。</summary>
        private void BuildScoreColumns(List<RuleDef> rules)
        {
            dgvScoreDetail.Columns.Clear();

            // 静态列：序号 / 学号 / 姓名（Tag 保持 null，用于在校验/保存/快速填充时与规则列区分）
            dgvScoreDetail.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = COL_INDEX,
                HeaderText = "序号",
                Width = 64,
                ReadOnly = true,
                Tag = null,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            dgvScoreDetail.Columns.Add(new DataGridViewTextBoxColumn { Name = COL_SID, HeaderText = "学号", Width = 140, ReadOnly = true, Tag = null });
            dgvScoreDetail.Columns.Add(new DataGridViewTextBoxColumn { Name = COL_SNAME, HeaderText = "姓名", Width = 110, ReadOnly = true, Tag = null });

            // 评分规则由后台查询阶段传入（含选项映射），此处只在 UI 线程建列
            foreach (var r in rules)
            {
                var opts = r.OptionMap;
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
                        Width = 120,
                        FlatStyle = FlatStyle.Flat,
                        DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
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
                        Width = 120,
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

        /// <summary>
        /// 切卡片时异步刷新右侧表：后台线程查数据，回 UI 线程填表。
        /// 用 _loadToken 只应用最后一次切换，丢弃过期回调，避免快速点卡片错乱。
        /// </summary>
        private async void RefreshScoreGridAsync(CourseItem item)
        {
            int token = ++_loadToken;
            GridData? data;
            try
            {
                data = await Task.Run(() => LoadGridData(item));
            }
            catch (Exception ex)
            {
                if (token == _loadToken)
                    MessageBox.Show("加载学生数据失败：" + ex.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (token != _loadToken) return;                    // 期间又切了卡片，丢弃本次
            if (!ReferenceEquals(_currentCourse, item)) return; // 当前已不是这节课
            ApplyGridData(item, data!);
        }

        /// <summary>纯数据查询（后台线程执行）：评分规则+选项、班级学生、已存分数。</summary>
        private GridData LoadGridData(CourseItem item)
        {
            var data = new GridData();
            foreach (var r in LoadScoringRules())
                data.Rules.Add(new RuleDef { RuleName = r.RuleName, MaxScore = r.MaxScore, OptionMap = LoadRuleOptions(r.RuleName) });

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
                    data.Students.Add((reader.GetString(reader.GetOrdinal(S_StudentID)),
                                       reader.GetString(reader.GetOrdinal(S_StudentName))));
            }

            int logId = item.LogID ?? 0;
            string sql2 = $"SELECT {D_StudentID}, {D_RuleName}, {D_Score} FROM ScoreDetails WHERE {D_LogID} = @LogID";
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql2;
                cmd.Parameters.AddWithValue("@LogID", logId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string sid = reader.GetString(0);
                    string rule = reader.GetString(1);
                    double score = reader.IsDBNull(2) ? 0 : reader.GetDouble(2);
                    data.Saved[(sid, rule)] = score;
                }
            }
            return data;
        }

        /// <summary>回 UI 线程把查询结果填表：建列 -> 填学生行 -> 回填分数。全程屏蔽脏标记。</summary>
        private void ApplyGridData(CourseItem item, GridData data)
        {
            _loading = true;
            try
            {
                BuildScoreColumns(data.Rules);
                dgvScoreDetail.SuspendLayout();
                dgvScoreDetail.Rows.Clear();
                int idx = 1;
                foreach (var st in data.Students)
                {
                    int r = dgvScoreDetail.Rows.Add();
                    var row = dgvScoreDetail.Rows[r];
                    row.Cells[COL_INDEX].Value = idx++;
                    row.Cells[COL_SID].Value = st.Sid;
                    row.Cells[COL_SNAME].Value = st.Name;
                }
                BackfillSavedScores(data);
                dgvScoreDetail.ResumeLayout(false);
                dgvScoreDetail.ClearSelection();
                if (dgvScoreDetail.Rows.Count > 0 && dgvScoreDetail.Columns.Count > 3)
                {
                    // 光标默认落在第一个评分列，配合 EditOnEnter 可立即键盘打分
                    dgvScoreDetail.CurrentCell = dgvScoreDetail.Rows[0].Cells[3];
                }
            }
            finally
            {
                _loading = false;
            }
        }

        /// <summary>回填该节课已保存过的分数：ScoreDetails 已有记录则填回表格（下拉按分值反查文本）。</summary>
        private void BackfillSavedScores(GridData data)
        {
            // (StudentID, RuleName) -> Score（数据已由后台查询阶段提供，这里只做 UI 回填）
            var saved = data.Saved;
            foreach (DataGridViewRow row in dgvScoreDetail.Rows)
            {
                string? sid = row.Cells[COL_SID].Value?.ToString();
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

        private void dgvScoreDetail_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
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
        // 三·补、Excel 式快速填写（B 复制粘贴 / C 选区批量填充 / D 手感微调）
        // =====================================================================

        /// <summary>
        /// 窗体层命令键拦截：在编辑子控件吞键之前拿到 Ctrl+C / Ctrl+V / Ctrl+Enter，
        /// 仅当焦点在学生评分表（含其编辑框）时生效。
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (dgvScoreDetail.ContainsFocus)
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.C:
                        CopySelectionToClipboard();
                        return true;
                    case Keys.Control | Keys.V:
                        PasteFromClipboard();
                        return true;
                    case Keys.Control | Keys.Enter:
                        FillSelectionWithCurrentValue();
                        return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>编辑框出现时：数字框全选(打字即覆盖旧值)，并挂上 Enter/方向键换格逻辑。</summary>
        private void DgvScoreDetail_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // 只处理数字文本列；下拉(ComboBox)列保留其自身交互，不接管方向键。
            if (e.Control is TextBox tb)
            {
                tb.KeyDown -= ScoreEdit_KeyDown; // 防重复订阅
                tb.KeyDown += ScoreEdit_KeyDown;
                tb.SelectAll();                  // 进入编辑即全选 -> 打字直接覆盖（Excel 手感）
            }
        }

        /// <summary>编辑框内 Enter/方向键 -> 提交当前格并移动到相邻格（Excel 行为）。</summary>
        private void ScoreEdit_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter when !e.Shift && !e.Control:
                    e.SuppressKeyPress = true;
                    MoveActiveCell(+1, 0);
                    break;
                case Keys.Down:
                    e.SuppressKeyPress = true;
                    MoveActiveCell(+1, 0);
                    break;
                case Keys.Up:
                    e.SuppressKeyPress = true;
                    MoveActiveCell(-1, 0);
                    break;
                case Keys.Left:
                    e.SuppressKeyPress = true;
                    MoveActiveCell(0, -1);
                    break;
                case Keys.Right:
                    e.SuppressKeyPress = true;
                    MoveActiveCell(0, +1);
                    break;
            }
        }

        /// <summary>提交当前编辑并把活动单元格按 (dr,dc) 位移一格（越界自动夹取）。EditOnEnter 下新格自动进入编辑。</summary>
        private void MoveActiveCell(int dr, int dc)
        {
            var cur = dgvScoreDetail.CurrentCell;
            if (cur == null) return;
            int r = cur.RowIndex, c = cur.ColumnIndex;
            dgvScoreDetail.EndEdit(); // 先提交正在编辑的格（触发校验）
            r = Math.Max(0, Math.Min(dgvScoreDetail.Rows.Count - 1, r + dr));
            c = Math.Max(0, Math.Min(dgvScoreDetail.Columns.Count - 1, c + dc));
            if (r == cur.RowIndex && c == cur.ColumnIndex) return;
            dgvScoreDetail.CurrentCell = dgvScoreDetail.Rows[r].Cells[c];
        }

        /// <summary>B：把选区按 Excel 同款"Tab 分隔列、换行分隔行"写入剪贴板（用官方 GetClipboardContent）。</summary>
        private void CopySelectionToClipboard()
        {
            if (dgvScoreDetail.CurrentCell == null) return;
            dgvScoreDetail.EndEdit(); // 编辑态下先提交，避免只复制编辑框选中文本
            IDataObject? data = dgvScoreDetail.GetClipboardContent(); // 受 ClipboardCopyMode 控制，已设为不带表头
            if (data != null) Clipboard.SetDataObject(data, true);
        }

        /// <summary>C：把当前单元格值广播填充到整个选区内的可编辑评分格。</summary>
        private void FillSelectionWithCurrentValue()
        {
            if (dgvScoreDetail.CurrentCell == null) return;
            dgvScoreDetail.EndEdit();
            string text = dgvScoreDetail.CurrentCell.Value?.ToString() ?? "";
            int applied = 0, skipped = 0;
            foreach (DataGridViewCell cell in dgvScoreDetail.SelectedCells)
            {
                if (cell.RowIndex < 0) continue;
                if (cell == dgvScoreDetail.CurrentCell) continue; // 当前格已是该值
                if (TrySetScoreCell(cell, text, out bool changed)) { if (changed) applied++; }
                else skipped++;
            }
            if (applied > 0) SetSaveState(true);
            if (skipped > 0)
                MessageBox.Show($"已填充 {applied} 个单元格，跳过 {skipped} 个（只读列、不合法数字或不在该列选项中）。",
                    "批量填充");
        }

        /// <summary>B：解析剪贴板文本（Tab 分列、换行分行）粘回评分表。单值->铺满选区；多值->自选区左上角展开。</summary>
        private void PasteFromClipboard()
        {
            if (dgvScoreDetail.CurrentCell == null) return;
            string clip;
            try { clip = Clipboard.GetText(); } catch { return; }
            if (string.IsNullOrEmpty(clip)) return;
            dgvScoreDetail.EndEdit();

            if (!GetSelectionBounds(out int r1, out int c1))
            {
                var cc = dgvScoreDetail.CurrentCell;
                r1 = cc.RowIndex; c1 = cc.ColumnIndex;
            }

            // 规范化换行并去掉尾部多余空行
            var lines = clip.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd('\n').Split('\n');
            int applied = 0, skipped = 0;
            bool single = lines.Length == 1 && !lines[0].Contains('\t');

            if (single)
            {
                // 单个值 -> 广播到当前选区（若选区只有当前格，等同于只改当前格）
                string v = lines[0];
                foreach (DataGridViewCell cell in dgvScoreDetail.SelectedCells)
                {
                    if (cell.RowIndex < 0) continue;
                    if (TrySetScoreCell(cell, v, out bool changed)) { if (changed) applied++; }
                    else skipped++;
                }
            }
            else
            {
                // 多行/多列块 -> 以选区左上角为锚点，按行列顺序写入
                for (int i = 0; i < lines.Length; i++)
                {
                    int tr = r1 + i;
                    if (tr >= dgvScoreDetail.Rows.Count) break;
                    var parts = lines[i].Split('\t');
                    for (int j = 0; j < parts.Length; j++)
                    {
                        int tc = c1 + j;
                        if (tc >= dgvScoreDetail.Columns.Count) break;
                        var cell = dgvScoreDetail.Rows[tr].Cells[tc];
                        if (TrySetScoreCell(cell, parts[j], out bool changed)) { if (changed) applied++; }
                        else skipped++;
                    }
                }
            }

            if (applied > 0) SetSaveState(true);
            if (skipped > 0)
                MessageBox.Show($"粘贴完成：写入 {applied} 个单元格，跳过 {skipped} 个（只读列、不合法数字或不在该列选项中）。",
                    "粘贴");
        }

        /// <summary>取选区包围盒的左上角行列；无选中返回 false。</summary>
        private bool GetSelectionBounds(out int rowTop, out int colLeft)
        {
            rowTop = -1; colLeft = -1;
            if (dgvScoreDetail.SelectedCells.Count == 0) return false;
            foreach (DataGridViewCell cell in dgvScoreDetail.SelectedCells)
            {
                if (cell.RowIndex < 0) continue;
                if (rowTop < 0) { rowTop = cell.RowIndex; colLeft = cell.ColumnIndex; }
                else
                {
                    if (cell.RowIndex < rowTop) rowTop = cell.RowIndex;
                    if (cell.ColumnIndex < colLeft) colLeft = cell.ColumnIndex;
                }
            }
            return rowTop >= 0;
        }

        /// <summary>
        /// 安全写入一个评分单元格：只读静态列 / 下拉值不在选项 / 数字越界 一律拒绝(返回 false)。
        /// 程序化赋值不会触发 CellValidating，故在此就地校验。空串=清空。changed 标识是否真正改动。
        /// </summary>
        private bool TrySetScoreCell(DataGridViewCell cell, string raw, out bool changed)
        {
            changed = false;
            var col = dgvScoreDetail.Columns[cell.ColumnIndex];
            if (col.Tag is not RuleColumnTag tag) return false; // 序号/学号/姓名 -> 只读，跳过

            string text = (raw ?? "").Trim();
            object? old = cell.Value;

            if (tag.IsOption)
            {
                if (text.Length == 0) { cell.Value = null; }
                else
                {
                    if (!((DataGridViewComboBoxColumn)col).Items.Contains(text)) return false; // 不在选项 -> 跳过
                    cell.Value = text;
                }
            }
            else
            {
                if (text.Length == 0) { cell.Value = null; }
                else
                {
                    if (!double.TryParse(text, out double v) || v < 0 || v > tag.MaxScore) return false; // 非法数字 -> 跳过
                    cell.Value = text;
                }
            }

            changed = !object.Equals(cell.Value, old);
            return true;
        }

        // =====================================================================
        // 四、按钮动作
        // =====================================================================

        /// <summary>刷新：重新按当前课程建列 + 拉学生 + 回填。</summary>
        private void ReloadForCurrentCourse()
        {
            if (!ConfirmLeaveCourse()) return;
            if (HasCourse(_currentCourse))
            {
                RefreshScoreGridAsync(_currentCourse!);
                SetSaveState(false);
            }
            else
            {
                BuildCourseCards(_lastDate);
            }
        }

        /// <summary>全部出勤：把所有"文本选项"列填成该规则里分值最高的选项（通常为"到"）。</summary>
        private void FillAllPresent()
        {
            if (!HasCourse(_currentCourse)) { MessageBox.Show("请先在左侧选择一节有效课程。", "提示"); return; }

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
            SetSaveState(true);
        }

        /// <summary>
        /// 清除选定：清空被选中单元格所属行的打分组内容（保留学生行本身）。
        /// 评分表是 CellSelect 模式，SelectedRows 为空，需从 SelectedCells 反推涉及的行。
        /// </summary>
        private void ClearSelectedRowsScores()
        {
            if (!HasCourse(_currentCourse)) { MessageBox.Show("请先在左侧选择一节有效课程。", "提示"); return; }

            var ruleCols = dgvScoreDetail.Columns.Cast<DataGridViewColumn>().Where(c => c.Tag is RuleColumnTag).ToList();
            var rows = new HashSet<int>();
            foreach (DataGridViewCell cell in dgvScoreDetail.SelectedCells)
                if (cell.RowIndex >= 0) rows.Add(cell.RowIndex);

            if (rows.Count == 0)
            {
                MessageBox.Show("请先选中要清除评分的单元格或整行。", "提示");
                return;
            }

            _loading = true;   // 批量清空不算"逐格用户改动"，避免逐格触发脏标记，最后统一置脏
            try
            {
                foreach (int ri in rows)
                    foreach (var col in ruleCols)
                        dgvScoreDetail.Rows[ri].Cells[col.Name].Value = null;
            }
            finally { _loading = false; }

            SetSaveState(true);
        }

        /// <summary>保存数据：逐行逐规则拆分写入 ScoreDetails；存在(LogID,StudentID,RuleName)则更新，否则插入。单事务，出错整体回滚。</summary>
        private void ButtonSave_Click(object? sender, EventArgs e) => SaveCurrentCourse();

        private void SaveCurrentCourse()
        {
            if (!HasCourse(_currentCourse))
            {
                MessageBox.Show("请先在左侧选择一节有效课程。", "提示");
                return;
            }

            dgvScoreDetail.EndEdit(); // 确保正在编辑的单元格先提交
            int logId = _currentCourse!.LogID!.Value;
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
                SetSaveState(false);   // 保存成功 -> 状态灯转绿
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

            using var dlg = new KryptonForm
            {
                Text = "评分映射设置（文本选项 → 分值）",
                ClientSize = new Size(420, 380),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable
            };

            var lblRule = new Label { Text = "选择规则：", Location = new Point(12, 15), AutoSize = true };
            var cmbRule = new ComboBox { Left = 120, Top = 12, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
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

            var btnDel = new Button { Text = "删除选中行", Left = 12, Top = 310, Width = 110, Height = 34, Anchor = AnchorStyles.Left | AnchorStyles.Bottom };
            btnDel.Click += (s, e) =>
            {
                var sel = grid.SelectedRows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).ToList();
                foreach (var r in sel) grid.Rows.Remove(r);
            };

            var btnOk = new Button { Text = "保存", Left = 200, Top = 310, Width = 90, Height = 34, Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
            var btnCancel = new Button { Text = "关闭", Left = 306, Top = 310, Width = 90, Height = 34, Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
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
                if (HasCourse(_currentCourse))
                {
                    RefreshScoreGridAsync(_currentCourse!);
                    SetSaveState(false);
                }
                MessageBox.Show($"规则[{rule}] 的映射已保存。", "成功");
                LoadRuleToGrid();
            };

            dlg.Controls.AddRange(new Control[] { lblRule, cmbRule, grid, btnDel, btnOk, btnCancel });
            if (cmbRule.Items.Count > 0) cmbRule.SelectedIndex = 0;
            LoadRuleToGrid();
            dlg.ShowDialog(this);
        }

        // =====================================================================
        // 五·补、课程卡片（自绘圆角卡片，无第三方依赖）
        // =====================================================================

        /// <summary>
        /// 左侧课程卡片：自绘圆角面板 + 节次徽标 + 课程信息。
        /// 无课时置灰占位（Enabled=false、文字变灰、显示【本节无课程】）。
        /// 只用 GDI+（Graphics.DrawString / MeasureString），不使用 TextRenderer，
        /// 避免其重载在不同 .NET 版本间存在差异导致的编译不一致。
        /// </summary>
        private readonly List<CourseCard> _cards = new();
    }
}