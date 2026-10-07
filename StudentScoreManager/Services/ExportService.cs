
using MiniExcelLibs;
using Microsoft.Data.Sqlite;
using StudentScoreManager.Models;
using System.Data;
using System.Text;

namespace StudentScoreManager.Services
{
    /// <summary>
    /// 数据导出服务类。
    /// 支持基于条件的灵活导出。
    /// </summary>
    public class ExportService
    {
        private readonly DbHelper _dbHelper;

        public ExportService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// 导出所有模块数据到 Excel（带筛选）。
        /// 设计要点：
        /// 1) 每个模块都用 DataTable 承载，即使查询结果为空，MiniExcel 仍会写出表头行，
        ///    这样用户可以在空表上直接填写后再导入补充数据。
        /// 2) 所有 Sheet 一律创建（不再"空则跳过"），表头齐全便于回环导入。
        /// 3) 写完后用 ClosedXML 对每个 Sheet 做自动列宽（若未安装 ClosedXML 则优雅降级，不影响导出）。
        /// </summary>
        public bool ExportAllToExcel(string filePath,
            ClassExportFilter? classFilter = null,
            StudentExportFilter? studentFilter = null,
            TeachingLogExportFilter? logFilter = null,
            bool exportScoreDetails = true)
        {
            try
            {
                // 用 Dictionary 承载多个 Sheet，Key = Sheet 名，Value = DataTable。
                // 注意：不再用 "if (list.Any())" 判断，空表也放入，MiniExcel 会保留 DataTable 的列（表头）。
                var data = new Dictionary<string, object>();

                // 1. 班级管理
                data["班级管理"] = GetClassData(classFilter);

                // 2. 学生信息
                data["学生信息"] = GetStudentData(studentFilter);

                // 3. 教学日志
                data["教学日志"] = GetTeachingLogs(logFilter);

                // 4. 评分规则：不参与筛选，全量导出。空表也保留表头，便于"导出→改→再导入"回环。
                data["评分规则"] = GetScoringRules();

                // 5. 成绩明细：范围随教学日志筛选；改用中文表头，与其余 Sheet 风格一致。
                //    该 Sheet 数据量最大，是否导出由界面开关(exportScoreDetails)决定：
                //    关闭时整张 Sheet 不写入，其余四表照常导出，可显著减小导出文件体积、加快导出速度。
                if (exportScoreDetails)
                {
                    data["成绩明细"] = GetScoreDetails(logFilter);
                }

                MiniExcel.SaveAs(filePath, data, overwriteFile: true);

                // 自动列宽（后处理）。失败不影响导出主流程。
                TryAutoFitColumns(filePath);

                return true;
            }
            catch (IOException)
            {
                throw new Exception("导出失败：目标文件正在被 Excel 打开，请先关闭该文件后再试。");
            }
            catch (Exception ex)
            {
                throw new Exception($"导出失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 用 ClosedXML 对每个 Sheet 的所有列做"自动调整到内容宽度"。
        /// 通过 NuGet 引用 ClosedXML 后生效；若项目未引用该库，这里会捕获异常并跳过，导出的基础文件仍然可用。
        /// </summary>
        private void TryAutoFitColumns(string filePath)
        {
            try
            {
                // 需要 NuGet 安装 ClosedXML 包；正确的类是 XLWorkbook（不是 XLPackage，后者属 EPPlus）。
                using var package = new ClosedXML.Excel.XLWorkbook(filePath);
                foreach (var ws in package.Worksheets)
                {
                    ws.Columns().AdjustToContents();
                }
                package.Save();
            }
            catch
            {
                // 未引用 ClosedXML 或文件被占用时，保持 MiniExcel 已写出的文件原样，不阻断导出。
            }
        }

        #region 私有数据获取方法 (返回 DataTable，保证空结果仍带表头)

        private DataTable GetClassData(ClassExportFilter? filter)
        {
            var dt = new DataTable("班级管理");
            StringBuilder sqlBuilder = new StringBuilder("SELECT ClassName as 班级名称, Counselor as 辅导员, StudentCount as 班级人数 FROM Classes WHERE 1=1");
            var parameters = new Dictionary<string, object>();
            /*
             * 使用筛选
            if (!string.IsNullOrWhiteSpace(filter?.Keyword))
            {
                sqlBuilder.Append(" AND (ClassName LIKE @Keyword OR Counselor LIKE @Keyword)");
                parameters.Add("@Keyword", $"%{filter.Keyword}%");
            }*/
            sqlBuilder.Append(" ORDER BY ClassID");

            ExecuteQuery(sqlBuilder.ToString(), parameters, dt);
            return dt;
        }

        private DataTable GetStudentData(StudentExportFilter? filter)
        {
            var dt = new DataTable("学生信息");
            StringBuilder sqlBuilder = new StringBuilder("SELECT StudentID as 学号, StudentName as 姓名, ClassName as 所属班级 FROM Students WHERE 1=1");
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(filter?.ClassName))
            {
                sqlBuilder.Append(" AND ClassName = @ClassName");
                parameters.Add("@ClassName", filter.ClassName);
            }
            if (!string.IsNullOrWhiteSpace(filter?.Keyword))
            {
                sqlBuilder.Append(" AND (StudentName LIKE @Keyword OR StudentID LIKE @Keyword)");
                parameters.Add("@Keyword", $"%{filter.Keyword}%");
            }
            sqlBuilder.Append(" ORDER BY ClassName, StudentID");

            ExecuteQuery(sqlBuilder.ToString(), parameters, dt);
            return dt;
        }

        /// <summary>
        /// 构造教学日志的筛选 WHERE 片段与参数。
        /// 单一事实源：GetTeachingLogs 与 GetScoreDetails 都复用此方法，
        /// 保证"成绩明细范围"与"教学日志范围"逐字一致。
        /// </summary>
        private (string whereClause, Dictionary<string, object> parameters) BuildLogFilter(TeachingLogExportFilter? filter)
        {
            StringBuilder sqlBuilder = new StringBuilder(" WHERE 1=1");
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(filter?.ClassName))
            {
                sqlBuilder.Append(" AND ClassName = @ClassName");
                parameters.Add("@ClassName", filter.ClassName);
            }
            if (!string.IsNullOrWhiteSpace(filter?.CourseName))
            {
                sqlBuilder.Append(" AND CourseName LIKE @CourseName");
                parameters.Add("@CourseName", $"%{filter.CourseName}%");
            }
            if (filter?.StartDate.HasValue == true)
            {
                sqlBuilder.Append(" AND date(TeachingDate) >= date(@StartDate)");
                parameters.Add("@StartDate", filter.StartDate.Value.ToString("yyyy-MM-dd"));
            }
            if (filter?.EndDate.HasValue == true)
            {
                sqlBuilder.Append(" AND date(TeachingDate) <= date(@EndDate)");
                parameters.Add("@EndDate", filter.EndDate.Value.ToString("yyyy-MM-dd"));
            }

            return (sqlBuilder.ToString(), parameters);
        }

        private DataTable GetTeachingLogs(TeachingLogExportFilter? filter)
        {
            var dt = new DataTable("教学日志");
            var (whereClause, parameters) = BuildLogFilter(filter);

            StringBuilder sqlBuilder = new StringBuilder(
                "SELECT ClassName as 班级名称, CourseCode as 课程代码, CourseName as 课程名称, TeachingDate as 授课日期, TeachingContent as 教学内容, Classroom as 教室, TeachingHours as 节次 " +
                "FROM TeachingLogs");
            sqlBuilder.Append(whereClause);
            sqlBuilder.Append(" ORDER BY TeachingDate DESC");

            ExecuteQuery(sqlBuilder.ToString(), parameters, dt);
            return dt;
        }

        private DataTable GetScoringRules()
        {
            var dt = new DataTable("评分规则");
            // 表头与导入模板（评分规则 Sheet）保持一致，保证导出文件可被 ImportAll 直接回读。
            StringBuilder sqlBuilder = new StringBuilder(
                "SELECT RuleName as 评分维度, MaxScore as 最高评分, Weight as 权重, SortOrder as 排序 FROM ScoringRules WHERE 1=1");
            var parameters = new Dictionary<string, object>();
            sqlBuilder.Append(" ORDER BY SortOrder");

            ExecuteQuery(sqlBuilder.ToString(), parameters, dt);
            return dt;
        }

        /// <summary>
        /// 导出成绩明细（ScoreDetails）。
        /// 改动点：
        /// - 由 SELECT * 改为显式列出列并起中文别名，表头与其它 Sheet 风格统一。
        /// - 表不存在时，仍返回一个"只有中文表头的空表"，从而保证 Sheet 与表头一定存在，便于用户填写后再导入。
        /// </summary>
        private DataTable GetScoreDetails(TeachingLogExportFilter? logFilter)
        {
            // 中文表头 -> 数据库真实列名 的映射（顺序即导出列顺序）
            var headerMap = new (string cn, string en)[]
            {
                ("明细编号", "DetailID"),
                ("关联日志ID", "LogID"),
                ("学号", "StudentID"),
                ("评分维度", "RuleName"),
                ("得分", "Score"),
            };

            // 先构造"一定带表头"的空表骨架（中文列名）
            var dt = new DataTable("成绩明细");
            foreach (var (cn, _) in headerMap)
            {
                dt.Columns.Add(cn, typeof(object));
            }

            // 探测表是否存在（不同版本库可能没有 ScoreDetails 表）
            bool tableExists;
            using (var probe = _dbHelper.CreateConnection())
            {
                probe.Open();
                using var cmd = probe.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='ScoreDetails'";
                tableExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            if (!tableExists) return dt; // 返回只有表头的空表

            // 复用与教学日志完全一致的筛选，通过外键 LogID 把明细限定在日志范围内。
            var (whereClause, parameters) = BuildLogFilter(logFilter);

            // 显式列 + 中文别名，按明细稳定顺序输出。
            var selectCols = string.Join(", ", headerMap.Select(h => $"{h.en} as [{h.cn}]"));
            var sqlBuilder = new StringBuilder(
                $"SELECT {selectCols} FROM ScoreDetails WHERE LogID IN (SELECT LogID FROM TeachingLogs");
            sqlBuilder.Append(whereClause);
            sqlBuilder.Append(") ORDER BY LogID, StudentID, RuleName");

            // 查询结果列名即中文别名，直接填入 dt。
            ExecuteQueryInto(sqlBuilder.ToString(), parameters, dt);
            return dt;
        }

        /// <summary>
        /// 通用查询执行器：把结果填充进 DataTable（列名来自 SQL 别名，与 DataTable 列名一致）。
        /// 即使查询 0 行，DataTable 的列（表头）依旧存在。
        /// </summary>
        private void ExecuteQuery(string sql, Dictionary<string, object> parameters, DataTable dt)
        {
            ExecuteQueryInto(sql, parameters, dt);
        }

        private void ExecuteQueryInto(string sql, Dictionary<string, object> parameters, DataTable dt)
        {
            using var connection = _dbHelper.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            foreach (var param in parameters)
            {
                var p = command.CreateParameter();
                p.ParameterName = param.Key;
                p.Value = param.Value ?? DBNull.Value;
                command.Parameters.Add(p);
            }

            using var reader = command.ExecuteReader();

            // 若调用方未预置列（普通 Sheet），按 reader 返回的列名（中文别名）补建列。
            if (dt.Columns.Count == 0)
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    dt.Columns.Add(reader.GetName(i), typeof(object));
                }
            }

            while (reader.Read())
            {
                var row = dt.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string colName = reader.GetName(i);
                    if (!dt.Columns.Contains(colName))
                    {
                        dt.Columns.Add(colName, typeof(object));
                    }
                    row[colName] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                }
                dt.Rows.Add(row);
            }
        }

        #endregion
    }
}