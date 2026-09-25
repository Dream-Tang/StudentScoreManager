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

                // 4. 评分规则通常不需要筛选，直接全量导出或根据业务需求决定
                // var rules = GetScoringRules();
                // data["评分规则"] = rules;

                if (!data.Any())
                {
                    throw new Exception("没有符合筛选条件的数据可导出。");
                }

                MiniExcel.SaveAs(filePath, data, overwriteFile:true);
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

        private List<Dictionary<string, object>> GetTeachingLogs(TeachingLogExportFilter? filter)
        {
            var list = new List<Dictionary<string, object>>();

            StringBuilder sqlBuilder = new StringBuilder(
                "SELECT ClassName as 班级名称, CourseName as 课程名称, TeachingDate as 授课日期, TeachingContent as 教学内容, Classroom as 教室, TeachingHours as 节次 " +
                "FROM TeachingLogs WHERE 1=1");

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
                sqlBuilder.Append(" AND TeachingDate >= @StartDate");
                parameters.Add("@StartDate", filter.StartDate.Value.ToString("yyyy-MM-dd"));
            }

            if (filter?.EndDate.HasValue == true)
            {
                sqlBuilder.Append(" AND TeachingDate <= @EndDate");
                parameters.Add("@EndDate", filter.EndDate.Value.ToString("yyyy-MM-dd"));
            }

            sqlBuilder.Append(" ORDER BY TeachingDate DESC");

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
