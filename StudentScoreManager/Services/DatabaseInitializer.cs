// 文件路径: Services/DatabaseInitializer.cs

namespace StudentScoreManager.Services
{
    /// <summary>
    /// 数据库初始化服务。
    /// 负责在应用启动时检查并创建必要的数据表结构。
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly DbHelper _dbHelper;

        /// <summary>
        /// 初始化数据库构建器。
        /// </summary>
        /// <param name="dbHelper">由外部注入的数据库操作辅助类实例</param>
        public DatabaseInitializer(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// 执行数据库表的初始化创建。
        /// 使用 IF NOT EXISTS 确保重复启动应用时不会报错。
        /// </summary>
        public void InitializeTables()
        {
            try
            {
                // 1. 创建班级表
                string sqlClasses = @"CREATE TABLE IF NOT EXISTS Classes (
                                        ClassID INTEGER PRIMARY KEY AUTOINCREMENT,
                                        ClassName TEXT NOT NULL UNIQUE,
                                        Counselor TEXT,
                                        StudentCount INTEGER DEFAULT 0)";
                // 2. 创建学生表
                string sqlStudents = @"CREATE TABLE IF NOT EXISTS Students (
                                        StudentID TEXT PRIMARY KEY,
                                        StudentName TEXT NOT NULL,
                                        ClassName TEXT NOT NULL,
                                        FOREIGN KEY (ClassName) REFERENCES Classes(ClassName))";
                // 3. 创建评分规则表
                string sqlRules = @"CREATE TABLE IF NOT EXISTS ScoringRules (
                                        RuleID INTEGER PRIMARY KEY AUTOINCREMENT,
                                        RuleName TEXT NOT NULL,
                                        MaxScore REAL NOT NULL,
                                        Weight REAL NOT NULL,
                                        SortOrder INTEGER DEFAULT 0)";
                // 4. 创建教学日志表
                string sqlLogs = @"CREATE TABLE IF NOT EXISTS TeachingLogs (
                                        LogID INTEGER PRIMARY KEY AUTOINCREMENT,
                                        ClassName TEXT NOT NULL,
                                        CourseCode TEXT,
                                        CourseName TEXT NOT NULL,
                                        TeachingDate TEXT NOT NULL,
                                        TeachingHours TEXT NOT NULL,
                                        TeachingContent TEXT,
                                        Classroom TEXT,
                                        FOREIGN KEY (ClassName) REFERENCES Classes(ClassName))";
                // 5. 创建评分明细表
                string sqlScores = @"CREATE TABLE IF NOT EXISTS ScoreDetails (
                                        DetailID INTEGER PRIMARY KEY AUTOINCREMENT,
                                        LogID INTEGER NOT NULL,
                                        StudentID TEXT NOT NULL,
                                        RuleName TEXT NOT NULL,
                                        Score REAL NOT NULL,
                                        FOREIGN KEY (StudentID) REFERENCES Students(StudentID),
                                        FOREIGN KEY (LogID) REFERENCES TeachingLogs(LogID),
                                        FOREIGN KEY (RuleName) REFERENCES ScoringRules(RuleName))";

                // 依次执行建表语句
                _dbHelper.ExecuteNonQuery(sqlClasses);
                _dbHelper.ExecuteNonQuery(sqlStudents);
                _dbHelper.ExecuteNonQuery(sqlRules);
                _dbHelper.ExecuteNonQuery(sqlLogs);
                _dbHelper.ExecuteNonQuery(sqlScores);
            }
            catch (Exception ex)
            {
                throw new Exception($"数据库初始化失败: {ex.Message}", ex);
            }
        }
    }
}