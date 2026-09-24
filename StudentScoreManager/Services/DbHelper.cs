// 文件路径: Services/DbHelper.cs

using Microsoft.Data.Sqlite;
using System.Reflection;

namespace StudentScoreManager.Services
{
    /// <summary>
    /// 数据库操作辅助类。
    /// 封装了与 SQLite 数据库的底层交互，提供参数化的增删改查能力。
    /// </summary>
    public class DbHelper
    {
        // 数据库连接字符串（私有只读字段，确保线程安全）
        private readonly string _connectionString;

        /// <summary>
        /// 初始化数据库辅助类。
        /// </summary>
        /// <param name="dbPath">SQLite 数据库文件的路径</param>
        public DbHelper(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        // 暴露接口给外部进行使用
        public SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        /// <summary>
        /// 执行不返回结果集的 SQL 命令（如 INSERT, UPDATE, DELETE）。
        /// </summary>
        /// <param name="sql">参数化的 SQL 语句</param>
        /// <param name="param">匿名对象参数（例如：new { Name = "Test" }），内部通过反射映射为 SQL 参数</param>
        /// <returns>受影响的数据库行数</returns>
        public int ExecuteNonQuery(string sql, object? param = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            if (param != null)
            {
                BindParameters(command, param);
            }

            connection.Open();
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行查询并返回结果集中的第一行第一列。
        /// 常用于获取自增主键（last_insert_rowid()）或检查记录是否存在。
        /// </summary>
        /// <typeparam name="T">期望返回的数据类型</typeparam>
        /// <param name="sql">参数化的 SQL 语句</param>
        /// <param name="param">匿名对象参数</param>
        /// <returns>转换后的查询结果，若为空则返回类型的默认值</returns>
        public T ExecuteScalar<T>(string sql, object? param = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            if (param != null)
            {
                BindParameters(command, param);
            }

            connection.Open();
            var result = command.ExecuteScalar();

            // 1. 首先处理数据库返回的空值
            if (result == null || result == DBNull.Value)
            {
                return default(T)!; // 对于 int?，default(T) 就是 null，这是安全的
            }

            // 2. 如果 T 是可空类型（如 int?），我们需要先获取其基础类型（int）再进行转换
            var targetType = typeof(T);
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // 获取可空类型的基础类型，例如从 int? 获取 int
                targetType = Nullable.GetUnderlyingType(targetType)!;
            }

            // 3. 使用正确的基础类型进行转换
            return (T)Convert.ChangeType(result, targetType);
        }

        /// <summary>
        /// 内部辅助方法：通过反射将匿名对象的属性动态绑定为 SQL 命令的参数。
        /// </summary>
        /// <param name="command">SqliteCommand 实例</param>
        /// <param name="param">包含属性的匿名对象</param>
        private void BindParameters(SqliteCommand command, object param)
        {
            var properties = param.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                var value = prop.GetValue(param);
                var parameter = command.CreateParameter();

                // 约定：属性名必须与 SQL 中的参数名一致（如 @ClassName）
                parameter.ParameterName = $"@{prop.Name}";
                parameter.Value = value ?? DBNull.Value; // 将 C# 的 null 转换为数据库的 DBNull

                command.Parameters.Add(parameter);
            }
        }
    }
}