// 文件路径: Services/ImportService.cs

using Microsoft.Data.Sqlite;
using MiniExcelLibs;
using StudentScoreManager.Models;
using System.Transactions;

namespace StudentScoreManager.Services
{
    /// <summary>
    /// 数据导入服务类。
    /// 负责解析 Excel 文件，处理多表级联依赖，并在事务中安全地将数据写入 SQLite 数据库。
    /// </summary>
    public class ImportService
    {
        // 数据访问层实例
        private readonly DbHelper _dbHelper;

        /// <summary>
        /// 初始化导入服务。
        /// </summary>
        /// <param name="dbHelper">数据库操作辅助类实例</param>
        public ImportService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// 执行完整的数据导入流程。
        /// 整个流程包裹在事务中，确保数据的一致性（要么全部成功，要么全部回滚）。
        /// </summary>
        /// <param name="filePath">Excel 文件的绝对路径</param>
        /// <param name="overwriteExisting">如果为 true，则覆盖已存在的重复数据；如果为 false，则跳过重复数据并记录日志</param>
        /// <returns>包含导入结果和错误日志的 ImportResult 对象</returns>
        public ImportResult ImportAll(string filePath, bool overwriteExisting = false)
        {
            var result = new ImportResult();

            // 1. 前置校验：检查文件是否存在
            if (!File.Exists(filePath))
            {
                result.AddError("错误：指定的 Excel 文件不存在。");
                return result;
            }

            // 2. 开启事务作用域
            // TransactionScope 能够自动管理事务的提交与回滚
            using (var scope = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(5)))
            {
                try
                {
                    // 3. 读取 Excel 数据到内存
                    // 使用强类型模型读取 Excel 数据
                    var classes     = MiniExcel.Query<ClassImportModel>(filePath, sheetName: "班级管理").ToList();
                    var students    = MiniExcel.Query<StudentImportModel>(filePath, sheetName: "学生信息").ToList();
                    var logs        = MiniExcel.Query<TeachingLogImportModel>(filePath, sheetName: "教学日志").ToList();
                    var rules       = MiniExcel.Query<ScoreRuleImportModel>(filePath, sheetName: "评分规则").ToList();

                    // 按照外键依赖顺序执行导入
                    // 注意：将 overwriteExisting 参数传递下去
                    var classIdMap = ImportClasses(classes, result, overwriteExisting);     // 导入班级数据并获取班级名称到 ID 的映射
                    ImportStudents(students, classIdMap, result, overwriteExisting);        // 导入学生数据
                    ImportScoringRules(rules, result, overwriteExisting);                   // 导入评分规则数据
                    ImportTeachingLogs(logs, classIdMap, result, overwriteExisting);        // 导入教学日志数据

                    // 评估结果
                    if (result.ErrorLogs.Count == 0 && result.TotalSuccessCount > 0)
                    {
                        scope.Complete();
                        result.IsSuccess = true;
                    }
                    else if (result.TotalSuccessCount == 0 && result.ErrorLogs.Count == 0)
                    {
                        result.AddError("未能读取到有效数据。请检查 Excel 文件的表头是否与系统模板完全一致。");
                        result.IsSuccess = false;
                    }
                }
                catch (SqliteException sqlEx)
                {
                    // 专门处理数据库层面的错误
                    string friendlyMsg = "数据库操作失败";
                    if (sqlEx.Message.Contains("UNIQUE constraint failed"))
                    {
                        friendlyMsg = "数据已存在，请勿重复导入相同记录";
                    }
                    else if (sqlEx.Message.Contains("FOREIGN KEY constraint failed"))
                    {
                        friendlyMsg = "关联数据缺失，请确保先导入基础表（如班级管理）";
                    }
                    result.AddError($"系统错误：{friendlyMsg}");
                    result.IsSuccess = false;
                }
                catch (InvalidOperationException invEx)
                {
                    // 专门处理 MiniExcel 或类型转换错误（如 Int64 转 int? 失败）
                    if (invEx.Message.Contains("Invalid cast"))
                    {
                        result.AddError("数据格式错误：Excel 中存在非法数据或表头行被误读，请使用标准模板。");
                    }
                    else
                    {
                        result.AddError("数据格式错误：Excel 中存在非法数据或表头行被误读，请使用标准模板。");
                        result.AddError($"系统错误：{invEx.Message}");
                    }
                    result.IsSuccess = false;
                }
                catch (Exception ex)
                {
                    // 捕获其他所有未知错误
                    result.AddError($"系统致命错误：{ex.Message}");
                    result.IsSuccess = false;
                }
            }

            return result;
        }

        #region 私有导入方法

        /// <summary>
        /// 解析并导入班级数据。
        /// </summary>
        private Dictionary<string, int> ImportClasses(List<ClassImportModel> rows, ImportResult result, bool overwrite)
        {
            var classIdMap = new Dictionary<string, int>();
            int rowIndex = 2;

            if (rows == null) return classIdMap;

            foreach (var row in rows)
            {
                try
                {
                    // 使用 ?. 安全访问，并使用 ?? 提供空字符串作为默认值
                    string className = row.ClassName?.ToString()?.Trim() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(className)) { rowIndex++; continue; }

                    // 简单的防表头误读检查
                    if (className == "班级名称") { rowIndex++; continue; }

                    // Counselor 是可选字段，显式声明为 string? 允许为 null
                    string? counselor = row.Counselor?.ToString()?.Trim();
                    int? studentCount = row.StudentCount;

                    // 如果选择覆盖，直接尝试插入或替换，不需要先查询是否存在
                    if (overwrite)
                    {
                        // INSERT OR REPLACE 需要表中有 PRIMARY KEY 或 UNIQUE 约束
                        // 假设 ClassName 是唯一的，或者 ClassID 是自增主键但我们需要根据 ClassName 更新
                        // 注意：SQLite 的 INSERT OR REPLACE 实际上是 DELETE + INSERT，这会导致 ID 变化。
                        // 如果其他表引用了 ClassID，这可能会破坏外键关系。
                        // 更安全的做法是：先查 ID，如果有则 UPDATE，没有则 INSERT。

                        var existingId = _dbHelper.ExecuteScalar<int?>(
                            "SELECT ClassID FROM Classes WHERE ClassName = @ClassName",
                            new { ClassName = className });

                        if (existingId.HasValue)
                        {
                            // 更新现有记录
                            _dbHelper.ExecuteNonQuery(
                                "UPDATE Classes SET Counselor = @Counselor, StudentCount = @StudentCount WHERE ClassID = @ClassID",
                                new { ClassID = existingId.Value, Counselor = counselor, StudentCount = studentCount });

                            classIdMap[className] = existingId.Value;
                            // 可选：记录覆盖日志，或者静默处理
                            // result.AddWarning($"[班级管理] 第 {rowIndex} 行：班级 '{className}' 已更新。");
                        }
                        else
                        {
                            // 插入新记录
                            _dbHelper.ExecuteNonQuery(
                                "INSERT INTO Classes (ClassName, Counselor, StudentCount) VALUES (@ClassName, @Counselor, @StudentCount)",
                                new { ClassName = className, Counselor = counselor, StudentCount = studentCount });

                            int newId = _dbHelper.ExecuteScalar<int>("SELECT last_insert_rowid()");
                            classIdMap[className] = newId;
                        }
                    }
                    // 不覆盖模式
                    else
                    {
                        // 不覆盖模式：检查是否存在
                        var existingId = _dbHelper.ExecuteScalar<int?>(
                                                    "SELECT ClassID FROM Classes WHERE ClassName = @ClassName",
                                                    new { ClassName = className });

                        if (existingId.HasValue)
                        {
                            classIdMap[className] = existingId.Value;
                            result.AddError($"[班级管理] 第 {rowIndex} 行：班级 '{className}' 已存在，跳过导入。");
                        }
                        else
                        {
                            _dbHelper.ExecuteNonQuery(
                                "INSERT INTO Classes (ClassName, Counselor, StudentCount) VALUES (@ClassName, @Counselor, @StudentCount)",
                                new { ClassName = className, Counselor = counselor, StudentCount = studentCount });

                            int newId = _dbHelper.ExecuteScalar<int>("SELECT last_insert_rowid()");
                            classIdMap[className] = newId;
                        }
                    }

                    result.TotalSuccessCount++;

                    if (classIdMap.ContainsKey(className)) { rowIndex++; continue; }

                }
                catch (SqliteException sqlEx) when (sqlEx.Message.Contains("UNIQUE constraint failed"))
                {
                    result.AddError($"[班级管理] 第 {rowIndex} 行：班级 '{row.ClassName}' 已存在，跳过导入。");
                }
                catch (Exception ex)
                {
                    // 如果发生 Invalid cast，这里也能捕获并给出友好提示
                    string errorMsg = ex.Message.Contains("Invalid cast")
                        ? "数据格式错误（请检查是否为空或格式不对）"
                        : ex.Message;

                    result.AddError($"[班级管理] 第 {rowIndex} 行导入失败：{errorMsg}");
                }
                finally
                {
                    rowIndex++;
                }
            }

            return classIdMap;
        }

        /// <summary>
        /// 解析并导入学生数据。
        /// </summary>
        private void ImportStudents(List<StudentImportModel> rows, Dictionary<string, int> classIdMap, ImportResult result, bool overwrite)
        {
            int rowIndex = 2;
            if (rows == null) return;

            foreach (var row in rows)
            {
                try
                {
                    string studentId = row.StudentID?.ToString()?.Trim() ?? string.Empty;
                    string studentName = row.StudentName?.ToString()?.Trim() ?? string.Empty;
                    string className = row.ClassName?.ToString()?.Trim() ?? string.Empty;

                    // 增加防误读
                    if (studentId == "学号") { rowIndex++; continue; }

                    if (string.IsNullOrWhiteSpace(studentId) && string.IsNullOrWhiteSpace(studentName))
                    {
                        rowIndex++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(studentName))
                        throw new Exception("学号和姓名不能为空");
                    if (string.IsNullOrWhiteSpace(className))
                        throw new Exception("所属班级不能为空");

                    //if (!classIdMap.TryGetValue(className, out int className))
                    //    throw new Exception($"关联班级 '{className}' 不存在，请检查'班级管理'表");


                    // 覆盖模式
                    if (overwrite)
                    {
                        // 覆盖模式：先查是否存在，存在则 Update，不存在则 Insert
                        var existing = _dbHelper.ExecuteScalar<int?>(
                            "SELECT RowID FROM Students WHERE StudentID = @StudentID", // 假设 StudentID 是唯一标识
                            new { StudentID = studentId });

                        if (existing.HasValue)
                        {
                            _dbHelper.ExecuteNonQuery(
                                "UPDATE Students SET StudentName = @StudentName, ClassName = @ClassName WHERE StudentID = @StudentID",
                                new { StudentID = studentId, StudentName = studentName, ClassName = className });
                        }
                        else
                        {
                            _dbHelper.ExecuteNonQuery(
                                "INSERT INTO Students (StudentID, StudentName, ClassName) VALUES (@StudentID, @StudentName, @ClassName)",
                                new { StudentID = studentId, StudentName = studentName, ClassName = className });
                        }
                    }
                    // 不覆盖模式
                    else
                    {
                        var existing = _dbHelper.ExecuteScalar<int?>(
                           "SELECT StudentID FROM Students WHERE StudentID = @StudentID",
                           new { StudentID = studentId });

                        if (!existing.HasValue)
                        {
                            _dbHelper.ExecuteNonQuery(
                                "INSERT INTO Students (StudentID, StudentName, ClassName) VALUES (@StudentID, @StudentName, @ClassName)",
                                new { StudentID = studentId, StudentName = studentName, ClassName = className });
                        }
                        else
                        {
                            result.AddError($"[学生信息] 第 {rowIndex} 行：学号 '{studentId}' 已存在，跳过导入。");
                        }
                    }
                    result.TotalSuccessCount++;

                }
                catch (SqliteException ex) when (ex.Message.Contains("UNIQUE constraint failed"))
                {
                    result.AddError($"[学生信息] 第 {rowIndex} 行：学号 '{row.StudentID}' 已存在。");
                }
                catch (Exception ex)
                {
                    result.AddError($"[学生信息] 第 {rowIndex} 行导入失败：{ex.Message}");
                }
                finally
                {
                    rowIndex++;
                }
            }
        }

        private void ImportScoringRules(List<ScoreRuleImportModel> rows, ImportResult result, bool overwrite)
        {
            int rowIndex = 2;
            if (rows == null) return;

            foreach (var row in rows)
            {
                try
                {
                    // --- 修复点 1：更安全的字符串获取方式 ---
                    // 不要直接用 row.RuleName?.ToString()，这可能会把数字转成字符串，或者处理不好 DBNull
                    // 如果 RuleName 在模型里是 string，MiniExcel 应该已经转好了。
                    // 但为了保险，我们手动 Trim 并处理 null
                    string ruleName = (row.RuleName ?? string.Empty).ToString().Trim();

                    // --- 调试：打印实际读取到的值（你可以暂时保留，看输出窗口）---
                    // System.Diagnostics.Debug.WriteLine($"读取到的规则名: '{ruleName}', 原始类型: {row.RuleName?.GetType()}");

                    // --- 修复点 2：防误读表头 ---
                    if (ruleName == "评分维度") { rowIndex++; continue; }

                    if (string.IsNullOrWhiteSpace(ruleName))
                    {
                        rowIndex++;
                        continue;
                    }

                    // --- 基础校验 ---
                    if (!row.MaxScore.HasValue) throw new Exception("最高评分不能为空");
                    if (!row.Weight.HasValue) throw new Exception("权重不能为空");
                    if (!row.SortOrder.HasValue) throw new Exception("排序不能为空");
                    if (row.MaxScore <= 0) throw new Exception("满分必须大于0");
                    if (row.Weight < 0 || row.Weight > 1) throw new Exception("权重必须在 0 到 1 之间");

                    // 覆盖模式导入数据
                    if (overwrite)
                    {
                        // 覆盖模式：使用 INSERT OR REPLACE 或者 Update+Insert 逻辑
                        var existing = _dbHelper.ExecuteScalar<string?>(
                            "SELECT RuleName FROM ScoringRules WHERE TRIM(RuleName) = @RuleName",
                            new { RuleName = ruleName });

                        if (!string.IsNullOrEmpty(existing))
                        {
                            _dbHelper.ExecuteNonQuery(
                                "UPDATE ScoringRules SET MaxScore = @MaxScore, Weight = @Weight, SortOrder = @SortOrder WHERE RuleName = @RuleName",
                                new { RuleName = existing, MaxScore = row.MaxScore, Weight = row.Weight, SortOrder = row.SortOrder });
                        }
                        else
                        {
                            _dbHelper.ExecuteNonQuery(
                                "INSERT INTO ScoringRules (RuleName, MaxScore, Weight, SortOrder) VALUES (@RuleName, @MaxScore, @Weight, @SortOrder)",
                                new { RuleName = ruleName, MaxScore = row.MaxScore, Weight = row.Weight, SortOrder = row.SortOrder });
                        }
                    }
                    // 不覆盖模式导入数据
                    else
                    {
                        // 不覆盖模式
                        var existing = _dbHelper.ExecuteScalar<string>(
                            "SELECT RuleName FROM ScoringRules WHERE TRIM(RuleName) = @RuleName",// 数据库端也 Trim 一下，防止数据库里有空格
                            new { RuleName = ruleName });

                        if (!string.IsNullOrEmpty(existing))
                        {   // 如果查到了，说明重复了
                            throw new Exception($"维度'{ruleName}' 已存在，请勿重复导入");
                        }
                        // --- 执行插入 ---
                        _dbHelper.ExecuteNonQuery(
                            "INSERT INTO ScoringRules (RuleName, MaxScore, Weight, SortOrder) VALUES (@RuleName, @MaxScore, @Weight, @SortOrder)",
                            new { RuleName = ruleName, MaxScore = row.MaxScore, Weight = row.Weight, SortOrder = row.SortOrder });
                    }
                 }
                catch (SqliteException sqlEx) when (sqlEx.Message.Contains("UNIQUE constraint failed"))
                {
                    result.AddError($"[评分规则] 第 {rowIndex} 行：维度 '{row.RuleName}' 数据库已存在（唯一约束冲突）。");
                }
                catch (Exception ex)
                {
                    result.AddError($"[评分规则] 第 {rowIndex} 行导入失败：{ex.Message}");
                }
                finally
                {
                    rowIndex++;
                }
            }
        }

        /// <summary>
        /// 解析并导入教学日志数据。
        /// 注意：教学日志通常没有唯一的业务主键（除非组合键），这里假设每次导入都是新增，或者根据日期+班级+课程判断重复。
        /// 为了简化，这里暂时只实现“跳过完全相同的记录”或“全部新增”。
        /// 如果需要覆盖教学日志，逻辑会比较复杂，建议默认不覆盖或仅追加。
        /// </summary>
        private void ImportTeachingLogs(List<TeachingLogImportModel> rows, Dictionary<string, int> classIdMap, ImportResult result, bool overwrite)
        {
            int rowIndex = 2;
            if (rows == null) return;

            foreach (var row in rows)
            {
                try
                {
                    string className = row.ClassName?.ToString()?.Trim() ?? string.Empty;
                    string courseName = row.CourseName?.ToString()?.Trim() ?? string.Empty;
                    string teachingDate = row.TeachingDate?.ToString()?.Trim() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(className) && string.IsNullOrWhiteSpace(courseName))
                    {
                        rowIndex++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(className)) throw new Exception("班级不能为空");
                    if (string.IsNullOrWhiteSpace(courseName)) throw new Exception("课程名称不能为空");
                    if (string.IsNullOrWhiteSpace(teachingDate)) throw new Exception("授课日期不能为空");

                    if (!classIdMap.TryGetValue(className, out int classId))
                        throw new Exception($"关联班级 '{className}' 不存在");

                    //if (!DateTime.TryParse(teachingDate, out _))
                    //    throw new Exception($"授课日期 '{teachingDate}' 格式不正确，请使用 YYYY-MM-DD 格式");

                    // 可选字段：使用 ?? string.Empty 确保即使 Excel 单元格为空，也不会向数据库传入 null
                    string courseCode       = row.CourseCode?.ToString()?.Trim() ?? string.Empty;
                    string teachingContent  = row.TeachingContent?.ToString()?.Trim() ?? string.Empty;
                    string classroom        = row.Classroom?.ToString()?.Trim() ?? string.Empty;

                    string teachingHours = string.Empty;
                    if (row.TeachingHours != null)
                    {
                        teachingHours = row.TeachingHours.ToString()?.Trim() ?? string.Empty    ;
                    }

                    // 教学日志的覆盖逻辑：假设由 ClassName, CourseName, TeachingDate 组成唯一业务键
                    if (overwrite)
                    {
                        // 检查是否存在相同日期、相同班级、相同课程的日志
                        var existingLogId = _dbHelper.ExecuteScalar<int?>(
                            @"SELECT LogID FROM TeachingLogs 
                               WHERE ClassName = @ClassName AND CourseName = @CourseName AND TeachingDate = @TeachingDate AND TeachingHours = @TeachingHours",
                            new { ClassName = className, CourseName = courseName, TeachingDate = teachingDate, TeachingHours = teachingHours });
                        // 有重复的则更新条目
                        if (existingLogId.HasValue)
                        {
                            _dbHelper.ExecuteNonQuery(
                                @"UPDATE TeachingLogs 
                                   SET CourseCode = @CourseCode, TeachingContent = @TeachingContent, Classroom = @Classroom
                                   WHERE LogID = @LogID",
                                new
                                {
                                    LogID = existingLogId.Value,
                                    CourseCode = courseCode,
                                    TeachingContent = teachingContent,
                                    Classroom = classroom,
                                });
                        }
                        // 否则插入数据
                        else
                        {
                            _dbHelper.ExecuteNonQuery(
                                @"INSERT INTO TeachingLogs 
                                  (ClassName, CourseCode, CourseName, TeachingDate, TeachingContent, Classroom, TeachingHours) 
                                  VALUES 
                                  (@ClassName, @CourseCode, @CourseName, @TeachingDate, @TeachingContent, @Classroom, @TeachingHours)",
                                new
                                {
                                    ClassName = className,
                                    CourseCode = courseCode,
                                    CourseName = courseName,
                                    TeachingDate = teachingDate,
                                    TeachingContent = teachingContent,
                                    Classroom = classroom,
                                    TeachingHours = teachingHours
                                });
                        }
                    }
                    // 不覆盖，直接插入数据
                    else
                    {
                        // 不覆盖：直接插入，如果违反唯一约束（如果有）会报错，否则就是追加
                        _dbHelper.ExecuteNonQuery(
                            @"INSERT INTO TeachingLogs 
                              (ClassName, CourseCode, CourseName, TeachingDate, TeachingContent, Classroom, TeachingHours) 
                              VALUES 
                              (@ClassName, @CourseCode, @CourseName, @TeachingDate, @TeachingContent, @Classroom, @TeachingHours)",
                            new
                            {
                                ClassName = className,
                                CourseCode = courseCode,
                                CourseName = courseName,
                                TeachingDate = teachingDate,
                                TeachingContent = teachingContent,
                                Classroom = classroom,
                                TeachingHours = teachingHours
                            });
                    }
                    result.TotalSuccessCount++;
                }
                catch (Exception ex)
                {
                    result.AddError($"[教学日志] 第 {rowIndex} 行导入失败：{ex.Message}");
                }
                finally
                {
                    rowIndex++;
                }
            }
        }

        #endregion
    }
}