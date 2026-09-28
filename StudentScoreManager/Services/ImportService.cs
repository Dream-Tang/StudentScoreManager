
// 文件路径: Services/ImportService.cs
using Microsoft.Data.Sqlite;
using MiniExcelLibs;
using StudentScoreManager.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StudentScoreManager.Services
{
    /// <summary>
    /// 数据导入服务类。
    /// 负责解析 Excel 文件，处理多表级联依赖，并安全地将数据写入 SQLite 数据库。
    ///
    /// 事务模型（方案A·表级原子）：
    /// 放弃 TransactionScope（其在 Microsoft.Data.Sqlite 上默认不会把各自新建连接登记进隐式事务，
    /// Complete() 只是"假原子性"）。改为每个表的导入各开一对 SqliteConnection + SqliteTransaction，
    /// 表内每条写库透传同一连接与事务：表内全部成功则 Commit；表内任一行发生真异常则 Rollback，
    /// 该表整体回滚、已提交成功的其它表不受牵连。
    /// 顺带修复 P0-2：INSERT 与 last_insert_rowid() 复用同一连接，返回真实自增 ID。
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
        /// 读取 Excel 后，按外键依赖顺序逐表导入；每张表各自原子（表级事务）。
        /// </summary>
        /// <param name="filePath">Excel 文件的绝对路径</param>
        /// <param name="overwriteExisting">如果为 true，则覆盖已存在的重复数据；如果为 false，则跳过重复数据并记录日志</param>
        /// <returns>包含三态结果(新增/跳过/失败)的 ImportResult 对象</returns>
        public ImportResult ImportAll(string filePath, bool overwriteExisting = false)
        {
            var result = new ImportResult();

            // 1. 前置校验：检查文件是否存在
            if (!File.Exists(filePath))
            {
                result.AddError("错误：指定的 Excel 文件不存在。");
                result.IsSuccess = false;
                return result;
            }

            // 2. 读取 Excel 数据到内存（在事务之外；读取失败与导入事务无关）
            List<ClassImportModel> classes;
            List<StudentImportModel> students;
            List<TeachingLogImportModel> logs;
            List<ScoreRuleImportModel> rules;
            try
            {
                classes = MiniExcel.Query<ClassImportModel>(filePath, sheetName: "班级管理").ToList();
                students = MiniExcel.Query<StudentImportModel>(filePath, sheetName: "学生信息").ToList();
                logs = MiniExcel.Query<TeachingLogImportModel>(filePath, sheetName: "教学日志").ToList();
                rules = MiniExcel.Query<ScoreRuleImportModel>(filePath, sheetName: "评分规则").ToList();
            }
            catch (InvalidOperationException invEx)
            {
                result.AddError("数据格式错误：Excel 中存在非法数据或表头行被误读，请使用标准模板。");
                result.AddError($"系统错误：{invEx.Message}");
                result.IsSuccess = false;
                return result;
            }
            catch (Exception ex)
            {
                result.AddError($"读取 Excel 失败：{ex.Message}");
                result.IsSuccess = false;
                return result;
            }

            // 3. 按外键依赖顺序逐表导入。
            var classIdMap = new Dictionary<string, int>();

            try { ImportClasses(classes, classIdMap, result, overwriteExisting); }
            catch (Exception ex)
            {
                result.HasRolledBack = true;
                result.AddNotice($"[班级管理] 该表导入失败，已整体回滚（本次未写入任何数据）。失败原因：{ex.Message}");
            }
            try { ImportStudents(students, classIdMap, result, overwriteExisting); }
            catch (Exception ex)
            {
                result.HasRolledBack = true;
                result.AddNotice($"[学生信息] 该表导入失败，已整体回滚（本次未写入任何数据）。失败原因：{ex.Message}");
            }
            try { ImportScoringRules(rules, result, overwriteExisting); }
            catch (Exception ex)
            {
                result.HasRolledBack = true;
                result.AddNotice($"[评分规则] 该表导入失败，已整体回滚（本次未写入任何数据）。失败原因：{ex.Message}");
            }
            try { ImportTeachingLogs(logs, classIdMap, result, overwriteExisting); }
            catch (Exception ex)
            {
                result.HasRolledBack = true;
                result.AddNotice($"[教学日志] 该表导入失败，已整体回滚（本次未写入任何数据）。失败原因：{ex.Message}");
            }

            // 4. 结果判定（P0-4）：只要没有真异常即成功——"跳过"不再否决提交。
            result.TotalSuccessCount = result.Inserted;
            result.IsSuccess = result.ErrorLogs.Count == 0 && !result.HasRolledBack;

            if (result.Inserted == 0 && result.Skipped == 0 && result.Updated == 0 && result.ErrorLogs.Count == 0 && !result.HasRolledBack)
            {
                result.AddNotice("未能读取到有效数据。请检查 Excel 文件的表头是否与系统模板完全一致。");
                result.IsSuccess = false;
            }

            return result;
        }

        /// <summary>
        /// 一次性清理历史教学日志中的重复条目（P1）。
        /// 去重键与导入一致：班级 + 课程名 + 授课日期 + 节次(TeachingHours)。
        /// 每个业务键仅保留 LogID 最小（最早入库）的一条，其余重复行整体删除。
        /// 使用单独的连接与事务：全部删除成功后一次性提交，任何异常回滚。
        /// </summary>
        /// <returns>被删除的重复记录条数</returns>
        public int CleanupDuplicateTeachingLogs()
        {
            using var conn = _dbHelper.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                // 先统计将被删除的重复条数（每个业务键保留 MIN(LogID)）
                int toDelete = _dbHelper.ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM TeachingLogs
                      WHERE LogID NOT IN (
                          SELECT MIN(LogID) FROM TeachingLogs
                          GROUP BY ClassName, CourseName, date(TeachingDate), TeachingHours
                      )", null, conn, trans);

                if (toDelete > 0)
                {
                    _dbHelper.ExecuteNonQuery(
                        @"DELETE FROM TeachingLogs
                          WHERE LogID NOT IN (
                              SELECT MIN(LogID) FROM TeachingLogs
                              GROUP BY ClassName, CourseName, date(TeachingDate), TeachingHours
                          )", null, conn, trans);
                }

                trans.Commit();
                return toDelete;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        #region 私有导入方法

        /// <summary>
        /// 解析并导入班级数据（表级事务）。
        /// classIdMap 由调用方传入并在此填充；即使本表回滚，map 中的 key 仍可反映 Excel 出现过的班级。
        /// </summary>
        private void ImportClasses(List<ClassImportModel> rows, Dictionary<string, int> classIdMap, ImportResult result, bool overwrite)
        {
            if (rows == null) return;
            using var conn = _dbHelper.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            int rowIndex = 2;
            int inserted = 0;
            var skipped = new List<string>();
            int updated = 0;
            int failed = 0; // 遇错即记：循环内累计失败行数，循环结束后统一判定

            try
            {
                foreach (var row in rows)
                {
                    string className = string.Empty; // 提到 try 外，供 catch 中的失败/跳过提示引用
                    try
                    {
                        className = row.ClassName?.ToString()?.Trim() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(className)) { rowIndex++; continue; }
                        if (className == "班级名称") { rowIndex++; continue; }

                        string? counselor = row.Counselor?.ToString()?.Trim();
                        int? studentCount = row.StudentCount;

                        if (overwrite)
                        {
                            var existingId = _dbHelper.ExecuteScalar<int?>(
                                "SELECT ClassID FROM Classes WHERE ClassName = @ClassName",
                                new { ClassName = className }, conn, trans);
                            if (existingId.HasValue)
                            {
                                _dbHelper.ExecuteNonQuery(
                                    "UPDATE Classes SET Counselor = @Counselor, StudentCount = @StudentCount WHERE ClassID = @ClassID",
                                    new { ClassID = existingId.Value, Counselor = counselor, StudentCount = studentCount }, conn, trans);
                                classIdMap[className] = existingId.Value;
                                updated++;
                            }
                            else
                            {
                                _dbHelper.ExecuteNonQuery(
                                    "INSERT INTO Classes (ClassName, Counselor, StudentCount) VALUES (@ClassName, @Counselor, @StudentCount)",
                                    new { ClassName = className, Counselor = counselor, StudentCount = studentCount }, conn, trans);
                                int newId = _dbHelper.ExecuteScalar<int>("SELECT last_insert_rowid()", null, conn, trans);
                                classIdMap[className] = newId;
                                inserted++;
                            }
                        }
                        else
                        {
                            var existingId = _dbHelper.ExecuteScalar<int?>(
                                "SELECT ClassID FROM Classes WHERE ClassName = @ClassName",
                                new { ClassName = className }, conn, trans);
                            if (existingId.HasValue)
                            {
                                classIdMap[className] = existingId.Value;
                                skipped.Add($"[班级管理] 第 {rowIndex} 行：班级 '{className}' 已存在，跳过导入。");
                            }
                            else
                            {
                                _dbHelper.ExecuteNonQuery(
                                    "INSERT INTO Classes (ClassName, Counselor, StudentCount) VALUES (@ClassName, @Counselor, @StudentCount)",
                                    new { ClassName = className, Counselor = counselor, StudentCount = studentCount }, conn, trans);
                                int newId = _dbHelper.ExecuteScalar<int>("SELECT last_insert_rowid()", null, conn, trans);
                                classIdMap[className] = newId;
                                inserted++;
                            }
                        }
                    }
                    catch (SqliteException sqlEx) when (sqlEx.Message.Contains("UNIQUE constraint failed"))
                    {
                        skipped.Add($"[班级管理] 第 {rowIndex} 行：班级 '{className}' 已存在，跳过导入。");
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = ex.Message.Contains("Invalid cast")
                            ? "数据格式错误（请检查是否为空或格式不对）"
                            : ex.Message;
                        result.AddError($"[班级管理] 第 {rowIndex} 行（班级「{className}」）导入失败：{errorMsg}");
                        failed++;
                    }
                    finally
                    {
                        rowIndex++;
                    }
                }

                // 循环内不再"遇错即抛"：失败行已在 catch 记录到 result.Errors 并 failed++。
                // 循环结束后统一判定：有任一失败 → 整表回滚（丢弃本表全部未提交写入）；全部通过 → 提交。
                if (failed > 0)
                {
                    throw new Exception($"共 {failed} 行数据校验或写入失败，本表已整体回滚（本次未写入任何数据）。");
                }

                trans.Commit();
                result.Inserted += inserted;
                result.Updated += updated;
                foreach (var s in skipped) result.AddSkipped(s);
            }
            catch
            {
                // 兜底：回滚本表未提交写入后上抛（回滚本身再出错也忽略，保证异常继续上抛）。
                try { trans.Rollback(); } catch { }
                throw;
            }
        }

        /// <summary>
        /// 解析并导入学生数据（表级事务）。
        /// </summary>
        private void ImportStudents(List<StudentImportModel> rows, Dictionary<string, int> classIdMap, ImportResult result, bool overwrite)
        {
            if (rows == null) return;
            using var conn = _dbHelper.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            int rowIndex = 2;
            int inserted = 0;
            var skipped = new List<string>();
            int updated = 0;
            int failed = 0; // 遇错即记：循环内累计失败行数，循环结束后统一判定

            try
            {
                foreach (var row in rows)
                {
                    // 提到 try 外，供 catch 中的失败提示给出"是谁失败"的上下文
                    string studentId = string.Empty;
                    string studentName = string.Empty;
                    string className = string.Empty;
                    try
                    {
                        studentId = row.StudentID?.ToString()?.Trim() ?? string.Empty;
                        studentName = row.StudentName?.ToString()?.Trim() ?? string.Empty;
                        className = row.ClassName?.ToString()?.Trim() ?? string.Empty;

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

                        if (overwrite)
                        {
                            var existing = _dbHelper.ExecuteScalar<int?>(
                                "SELECT RowID FROM Students WHERE StudentID = @StudentID",
                                new { StudentID = studentId }, conn, trans);
                            if (existing.HasValue)
                            {
                                _dbHelper.ExecuteNonQuery(
                                    "UPDATE Students SET StudentName = @StudentName, ClassName = @ClassName WHERE StudentID = @StudentID",
                                    new { StudentID = studentId, StudentName = studentName, ClassName = className }, conn, trans);
                                updated++;
                            }
                            else
                            {
                                _dbHelper.ExecuteNonQuery(
                                    "INSERT INTO Students (StudentID, StudentName, ClassName) VALUES (@StudentID, @StudentName, @ClassName)",
                                    new { StudentID = studentId, StudentName = studentName, ClassName = className }, conn, trans);
                                inserted++;
                            }
                        }
                        else
                        {
                            var existing = _dbHelper.ExecuteScalar<int?>(
                                "SELECT StudentID FROM Students WHERE StudentID = @StudentID",
                                new { StudentID = studentId }, conn, trans);
                            if (!existing.HasValue)
                            {
                                _dbHelper.ExecuteNonQuery(
                                    "INSERT INTO Students (StudentID, StudentName, ClassName) VALUES (@StudentID, @StudentName, @ClassName)",
                                    new { StudentID = studentId, StudentName = studentName, ClassName = className }, conn, trans);
                                inserted++;
                            }
                            else
                            {
                                skipped.Add($"[学生信息] 第 {rowIndex} 行：学号 '{studentId}' 已存在，跳过导入。");
                            }
                        }
                    }
                    catch (SqliteException ex) when (ex.Message.Contains("UNIQUE constraint failed"))
                    {
                        skipped.Add($"[学生信息] 第 {rowIndex} 行：学号「{studentId}」已存在，跳过导入。");
                    }
                    catch (Exception ex)
                    {
                        result.AddError($"[学生信息] 第 {rowIndex} 行（学号「{studentId}」 姓名「{studentName}」 班级「{className}」）导入失败：{ex.Message}");
                        failed++;
                    }
                    finally
                    {
                        rowIndex++;
                    }
                }

                // 循环内不再"遇错即抛"：失败行已在 catch 记录到 result.Errors 并 failed++。
                // 循环结束后统一判定：有任一失败 → 整表回滚（丢弃本表全部未提交写入）；全部通过 → 提交。
                if (failed > 0)
                {
                    throw new Exception($"共 {failed} 行数据校验或写入失败，本表已整体回滚（本次未写入任何数据）。");
                }

                trans.Commit();
                result.Inserted += inserted;
                result.Updated += updated;
                foreach (var s in skipped) result.AddSkipped(s);
            }
            catch
            {
                // 兜底：回滚本表未提交写入后上抛（回滚本身再出错也忽略，保证异常继续上抛）。
                try { trans.Rollback(); } catch { }
                throw;
            }
        }

        /// <summary>
        /// 解析并导入评分规则数据（表级事务）。
        /// 非覆盖模式下"维度已存在"按"跳过"处理（不再抛异常回滚整表），与其它表语义一致。
        /// </summary>
        private void ImportScoringRules(List<ScoreRuleImportModel> rows, ImportResult result, bool overwrite)
        {
            if (rows == null) return;
            using var conn = _dbHelper.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            int rowIndex = 2;
            int inserted = 0;
            var skipped = new List<string>();
            int updated = 0;
            int failed = 0; // 遇错即记：循环内累计失败行数，循环结束后统一判定

            try
            {
                foreach (var row in rows)
                {
                    string ruleName = string.Empty; // 提到 try 外，供 catch 失败提示引用
                    try
                    {
                        ruleName = (row.RuleName ?? string.Empty).ToString().Trim();
                        if (ruleName == "评分维度") { rowIndex++; continue; }
                        if (string.IsNullOrWhiteSpace(ruleName))
                        {
                            rowIndex++;
                            continue;
                        }
                        if (!row.MaxScore.HasValue) throw new Exception("最高评分不能为空");
                        if (!row.Weight.HasValue) throw new Exception("权重不能为空");
                        if (!row.SortOrder.HasValue) throw new Exception("排序不能为空");
                        if (row.MaxScore <= 0) throw new Exception("满分必须大于0");
                        if (row.Weight < 0 || row.Weight > 1) throw new Exception("权重必须在 0 到 1 之间");

                        if (overwrite)
                        {
                            var existing = _dbHelper.ExecuteScalar<string?>(
                                "SELECT RuleName FROM ScoringRules WHERE TRIM(RuleName) = @RuleName",
                                new { RuleName = ruleName }, conn, trans);
                            if (!string.IsNullOrEmpty(existing))
                            {
                                _dbHelper.ExecuteNonQuery(
                                    "UPDATE ScoringRules SET MaxScore = @MaxScore, Weight = @Weight, SortOrder = @SortOrder WHERE RuleName = @RuleName",
                                    new { RuleName = existing, MaxScore = row.MaxScore, Weight = row.Weight, SortOrder = row.SortOrder }, conn, trans);
                                updated++;
                            }
                            else
                            {
                                _dbHelper.ExecuteNonQuery(
                                    "INSERT INTO ScoringRules (RuleName, MaxScore, Weight, SortOrder) VALUES (@RuleName, @MaxScore, @Weight, @SortOrder)",
                                    new { RuleName = ruleName, MaxScore = row.MaxScore, Weight = row.Weight, SortOrder = row.SortOrder }, conn, trans);
                                inserted++;
                            }
                        }
                        else
                        {
                            var existing = _dbHelper.ExecuteScalar<string>(
                                "SELECT RuleName FROM ScoringRules WHERE TRIM(RuleName) = @RuleName",
                                new { RuleName = ruleName }, conn, trans);
                            if (!string.IsNullOrEmpty(existing))
                            {
                                skipped.Add($"[评分规则] 第 {rowIndex} 行：维度 '{ruleName}' 已存在，跳过导入。");
                            }
                            else
                            {
                                _dbHelper.ExecuteNonQuery(
                                    "INSERT INTO ScoringRules (RuleName, MaxScore, Weight, SortOrder) VALUES (@RuleName, @MaxScore, @Weight, @SortOrder)",
                                    new { RuleName = ruleName, MaxScore = row.MaxScore, Weight = row.Weight, SortOrder = row.SortOrder }, conn, trans);
                                inserted++;
                            }
                        }
                    }
                    catch (SqliteException sqlEx) when (sqlEx.Message.Contains("UNIQUE constraint failed"))
                    {
                        skipped.Add($"[评分规则] 第 {rowIndex} 行：维度 '{ruleName}' 已存在，跳过导入。");
                    }
                    catch (Exception ex)
                    {
                        result.AddError($"[评分规则] 第 {rowIndex} 行（维度「{ruleName}」）导入失败：{ex.Message}");
                        failed++;
                    }
                    finally
                    {
                        rowIndex++;
                    }
                }

                // 循环内不再"遇错即抛"：失败行已在 catch 记录到 result.Errors 并 failed++。
                // 循环结束后统一判定：有任一失败 → 整表回滚（丢弃本表全部未提交写入）；全部通过 → 提交。
                if (failed > 0)
                {
                    throw new Exception($"共 {failed} 行数据校验或写入失败，本表已整体回滚（本次未写入任何数据）。");
                }

                trans.Commit();
                result.Inserted += inserted;
                result.Updated += updated;
                foreach (var s in skipped) result.AddSkipped(s);
            }
            catch
            {
                // 兜底：回滚本表未提交写入后上抛（回滚本身再出错也忽略，保证异常继续上抛）。
                try { trans.Rollback(); } catch { }
                throw;
            }
        }

        // 把 Excel 里读出来的任意日期形态清洗成统一 ISO 字符串
        private static string NormalizeTeachingDate(TeachingLogImportModel row)
        {
            var raw = row.TeachingDate;
            if (raw is DateTime dt)
                return dt.ToString("yyyy-MM-dd");

            string s = raw?.ToString()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(s))
                throw new Exception("授课日期不能为空");

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out DateTime d1))
                return d1.ToString("yyyy-MM-dd");

            // P0-3：捕获组与 Groups 索引一一对应——年/月/日各一组（月、日允许 1~2 位）
            var m = Regex.Match(s, @"(\d{4})[-/](\d{1,2})[-/](\d{1,2})");
            if (m.Success)
            {
                int y = int.Parse(m.Groups[1].Value);
                int mo = int.Parse(m.Groups[2].Value);
                int dd = int.Parse(m.Groups[3].Value);
                return new DateTime(y, mo, dd).ToString("yyyy-MM-dd");
            }
            throw new Exception($"授课日期 '{s}' 格式不正确，无法解析出年月日");
        }

        /// <summary>
        /// 解析并导入教学日志数据（表级事务）。
        /// 教学日志业务键 = 班级 + 课程名 + 授课日期 + 节次(TeachingHours)。
        /// 覆盖模式：命中业务键→更新(计"更新")，未命中→插入(计"新增")。
        /// 非覆盖模式：命中业务键→跳过(计"跳过")，未命中→插入(计"新增")。
        /// 去重不再依赖数据库唯一约束（TeachingLogs 无 UNIQUE），统一由程序先查一次。
        /// 授课日期比较统一用 date(TeachingDate)=date(@TeachingDate) 归一化（对齐 Form1 查询约定），
        /// 避免库内日期带时间/格式差异导致命中不到已有行 -> 覆盖时变成追加、改内容却不更新。
        /// </summary>
        private void ImportTeachingLogs(List<TeachingLogImportModel> rows, Dictionary<string, int> classIdMap, ImportResult result, bool overwrite)
        {
            if (rows == null) return;
            using var conn = _dbHelper.CreateConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            int rowIndex = 2;
            int inserted = 0;
            var skipped = new List<string>();
            int updated = 0;
            int failed = 0; // 遇错即记：循环内累计失败行数，循环结束后统一判定

            try
            {
                foreach (var row in rows)
                {
                    // 提到 try 外，供 catch 中失败提示给出班级/课程/日期/节次上下文
                    string className = string.Empty;
                    string courseName = string.Empty;
                    string teachingDate = string.Empty;
                    string teachingHours = string.Empty;
                    try
                    {
                        className = row.ClassName?.ToString()?.Trim() ?? string.Empty;
                        courseName = row.CourseName?.ToString()?.Trim() ?? string.Empty;
                        teachingDate = NormalizeTeachingDate(row);
                        teachingHours = row.TeachingHours?.ToString()?.Trim() ?? string.Empty;

                        // 查找excel的结束条件
                        if (string.IsNullOrWhiteSpace(className) && string.IsNullOrWhiteSpace(courseName)
                            && string.IsNullOrWhiteSpace(teachingDate) && string.IsNullOrWhiteSpace(teachingHours))
                        {
                            rowIndex++;
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(className)) throw new Exception("班级不能为空");
                        if (string.IsNullOrWhiteSpace(courseName)) throw new Exception("课程名称不能为空");
                        if (string.IsNullOrWhiteSpace(teachingDate)) throw new Exception("课程日期不能为空");
                        if (string.IsNullOrWhiteSpace(teachingHours)) throw new Exception("课程节次不能为空");
                        if (!classIdMap.TryGetValue(className, out int classId))
                            throw new Exception($"关联班级 '{className}' 不存在");

                        string courseCode = row.CourseCode?.ToString()?.Trim() ?? string.Empty;
                        string teachingContent = row.TeachingContent?.ToString()?.Trim() ?? string.Empty;
                        string classroom = row.Classroom?.ToString()?.Trim() ?? string.Empty;

                        if (overwrite)
                        {
                            var existingLogId = _dbHelper.ExecuteScalar<int?>(
                                @"SELECT LogID FROM TeachingLogs
                                    WHERE ClassName = @ClassName AND CourseName = @CourseName AND date(TeachingDate) = date(@TeachingDate) AND TeachingHours = @TeachingHours",
                                new { ClassName = className, CourseName = courseName, TeachingDate = teachingDate, TeachingHours = teachingHours }, conn, trans);
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
                                    }, conn, trans);
                                updated++;
                            }
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
                                    }, conn, trans);
                                inserted++;
                            }
                        }
                        else
                        {
                            // 非覆盖：先按业务键查一次，命中则跳过（修复"重复条目被继续追加"的问题）
                            var existingLogId = _dbHelper.ExecuteScalar<int?>(
                                @"SELECT LogID FROM TeachingLogs
                                    WHERE ClassName = @ClassName AND CourseName = @CourseName AND date(TeachingDate) = date(@TeachingDate) AND TeachingHours = @TeachingHours",
                                new { ClassName = className, CourseName = courseName, TeachingDate = teachingDate, TeachingHours = teachingHours }, conn, trans);
                            if (existingLogId.HasValue)
                            {
                                skipped.Add($"[教学日志] 第 {rowIndex} 行：班级「{className}」 课程「{courseName}」 日期「{teachingDate}」 节次「{teachingHours}」 已存在，跳过导入。");
                            }
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
                                    }, conn, trans);
                                inserted++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.AddError($"[教学日志] 第 {rowIndex} 行（班级「{className}」 课程「{courseName}」 日期「{teachingDate}」 节次「{teachingHours}」）导入失败：{ex.Message}");
                        failed++;
                    }
                    finally
                    {
                        rowIndex++;
                    }
                }

                // 循环内不再"遇错即抛"：失败行已在 catch 记录到 result.Errors 并 failed++。
                // 循环结束后统一判定：有任一失败 → 整表回滚（丢弃本表全部未提交写入）；全部通过 → 提交。
                if (failed > 0)
                {
                    throw new Exception($"共 {failed} 行数据校验或写入失败，本表已整体回滚（本次未写入任何数据）。");
                }

                trans.Commit();
                result.Inserted += inserted;
                result.Updated += updated;
                foreach (var s in skipped) result.AddSkipped(s);
            }
            catch
            {
                // 兜底：回滚本表未提交写入后上抛（回滚本身再出错也忽略，保证异常继续上抛）。
                try { trans.Rollback(); } catch { }
                throw;
            }
        }

        #endregion
    }
}