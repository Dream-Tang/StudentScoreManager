
using MiniExcelLibs;
using Microsoft.Data.Sqlite;
using StudentScoreManager.Models;
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
        /// 注意：如果某个模块的筛选结果为空，该 Sheet 仍将创建但为空，或者你可以选择跳过。
        /// </summary>
        public bool ExportAllToExcel(string filePath,
            ClassExportFilter? classFilter = null,
            StudentExportFilter? studentFilter = null,
            TeachingLogExportFilter? logFilter = null)
        {
            try
            {
                var data = new Dictionary<string, object>();

                // 1. 导出班级
                var classes = GetClassData(classFilter);
                if (classes.Any()) // 只有有数据才添加 Sheet，避免空表
                    data["班级管理"] = classes;

                // 2. 导出学生
                var students = GetStudentData(studentFilter);
                if (students.Any())
                    data["学生信息"] = students;

                // 3. 导出教学日志
                var logs = GetTeachingLogs(logFilter);
                if (logs.Any())
                    data["教学日志"] = logs;

                // 4. 评分规则不参与筛选，全量导出。补上该 Sheet 后，
                //     "导出→修改→再导入" 回环时 ImportAll 才能按固定表名读到「评分规则」。
                var rules = GetScoringRules();
                if (rules.Any())
                    data["评分规则"] = rules;

                // 5. 成绩明细（ScoreDetails）通过外键 LogID 关联教学日志：
                //    只导出「本次教学日志筛选范围」命中的日志所对应的成绩明细，
                //    不再整表全量导出。范围由 logFilter 决定，与教学日志 Sheet 严格一致。
                var details = GetScoreDetails(logFilter);
                if (details.Any())
                    data["成绩明细"] = details;

                if (!data.Any())
                {
                    throw new Exception("没有符合筛选条件的数据可导出。");
                }

                MiniExcel.SaveAs(filePath, data, overwriteFile: true);
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

        #region 私有数据获取方法 (支持动态 SQL)

        private List<Dictionary<string, object>> GetClassData(ClassExportFilter? filter)
        {
            var list = new List<Dictionary<string, object>>();

            // 基础 SQL
            StringBuilder sqlBuilder = new StringBuilder("SELECT ClassName as 班级名称, Counselor as 辅导员, StudentCount as 班级人数 FROM Classes WHERE 1=1");
            var parameters = new Dictionary<string, object>();

            // 动态添加条件
            if (!string.IsNullOrWhiteSpace(filter?.Keyword))
            {
                sqlBuilder.Append(" AND (ClassName LIKE @Keyword OR Counselor LIKE @Keyword)");
                parameters.Add("@Keyword", $"%{filter.Keyword}%");
            }

            sqlBuilder.Append(" ORDER BY ClassID");

            ExecuteQuery(sqlBuilder.ToString(), parameters, list);
            return list;
        }

        private List<Dictionary<string, object>> GetStudentData(StudentExportFilter? filter)
        {
            var list = new List<Dictionary<string, object>>();

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

            ExecuteQuery(sqlBuilder.ToString(), parameters, list);
            return list;
        }

        /// <summary>
        /// 构造教学日志的筛选 WHERE 片段与参数。
        /// 单一事实源：GetTeachingLogs（导出日志本身）与 GetScoreDetails（按日志范围过滤明细）
        /// 都复用此方法，保证"成绩明细范围"与"教学日志范围"逐字一致。
        /// 返回的 WHERE 以 " WHERE 1=1 ..." 开头，参数键与占位符一致。
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

        private List<Dictionary<string, object>> GetTeachingLogs(TeachingLogExportFilter? filter)
        {
            var list = new List<Dictionary<string, object>>();

            var (whereClause, parameters) = BuildLogFilter(filter);

            StringBuilder sqlBuilder = new StringBuilder(
                "SELECT ClassName as 班级名称, CourseCode as 课程代码, CourseName as 课程名称, TeachingDate as 授课日期, TeachingContent as 教学内容, Classroom as 教室, TeachingHours as 节次 " +
                "FROM TeachingLogs");
            sqlBuilder.Append(whereClause);
            sqlBuilder.Append(" ORDER BY TeachingDate DESC");

            ExecuteQuery(sqlBuilder.ToString(), parameters, list);
            return list;
        }

        private List<Dictionary<string, object>> GetScoringRules()
        {
            var list = new List<Dictionary<string, object>>();

            // 表头与导入模板（评分规则 Sheet）保持一致，保证导出文件可被 ImportAll 直接回读。
            StringBuilder sqlBuilder = new StringBuilder(
                "SELECT RuleName as 评分维度, MaxScore as 最高评分, Weight as 权重, SortOrder as 排序 FROM ScoringRules WHERE 1=1");
            var parameters = new Dictionary<string, object>();

            sqlBuilder.Append(" ORDER BY SortOrder");
            ExecuteQuery(sqlBuilder.ToString(), parameters, list);
            return list;
        }

        /// <summary>
        /// 导出成绩明细（ScoreDetails）。范围由教学日志筛选（logFilter）决定：
        /// ScoreDetails.LogID 是关联 TeachingLogs.LogID 的外键，因此用子查询
        ///   LogID IN (SELECT LogID FROM TeachingLogs <与教学日志完全相同的筛选>)
        /// 只导出落在本次日志范围内的明细，不再整表全量导出。
        /// 采用 SELECT * 动态取列：导出表头即数据库真实列名，配合导入侧通用回写实现无损回环。
        /// 兼容处理：若历史库中不存在 ScoreDetails 表，返回空列表而不抛异常，避免拖垮整个导出。
        /// </summary>
        private List<Dictionary<string, object>> GetScoreDetails(TeachingLogExportFilter? logFilter)
        {
            var list = new List<Dictionary<string, object>>();

            // 先探测表是否存在（不同版本库可能没有 ScoreDetails 表）
            using (var probe = _dbHelper.CreateConnection())
            {
                probe.Open();
                using var cmd = probe.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='ScoreDetails'";
                var exists = Convert.ToInt32(cmd.ExecuteScalar());
                if (exists == 0) return list;
            }

            // 复用与教学日志完全一致的筛选条件，通过外键 LogID 把明细限定在日志范围内。
            var (whereClause, parameters) = BuildLogFilter(logFilter);

            var sqlBuilder = new StringBuilder(
                "SELECT * FROM ScoreDetails WHERE LogID IN (SELECT LogID FROM TeachingLogs");
            sqlBuilder.Append(whereClause);
            sqlBuilder.Append(")");

            ExecuteQuery(sqlBuilder.ToString(), parameters, list);
            return list;
        }

        /// <summary>
        /// 通用查询执行器，将结果转换为字典列表
        /// </summary>
        private void ExecuteQuery(string sql, Dictionary<string, object> parameters, List<Dictionary<string, object>> resultList)
        {
            // 假设 DbHelper 有一个 CreateConnection 方法，或者我们直接使用连接字符串
            // 这里为了演示完整性，假设我们可以通过反射或公开属性获取连接，或者直接实例化
            // 建议：在 DbHelper 中公开 CreateConnection() 方法

            using var connection = _dbHelper.CreateConnection(); // 请确保 DbHelper 中有此方法
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            // 绑定参数
            foreach (var param in parameters)
            {
                var p = command.CreateParameter();
                p.ParameterName = param.Key;
                p.Value = param.Value ?? DBNull.Value;
                command.Parameters.Add(p);
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                resultList.Add(row);
            }
        }

        #endregion
    }
}