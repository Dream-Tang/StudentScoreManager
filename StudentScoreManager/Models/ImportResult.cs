// 文件路径: Models/ImportResult.cs

using System.Collections.Generic;

namespace StudentScoreManager.Models
{
    /// <summary>
    /// 数据导入操作的结果封装类。
    /// 用于向 UI 层传递导入任务的成功状态、成功记录数以及详细的错误日志。
    /// </summary>
    public class ImportResult
    {
        /// <summary>
        /// 获取或设置导入操作是否完全成功。
        /// 当且仅当没有任何致命错误且无数据校验错误时为 true。
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 获取或设置成功写入数据库的总记录数。
        /// </summary>
        public int TotalSuccessCount { get; set; }

        /// <summary>
        /// 获取或设置导入过程中的错误日志列表。
        /// 每条记录通常包含行号和具体的错误原因。
        /// </summary>
        public List<string> ErrorLogs { get; set; } = new List<string>();

        /// <summary>
        /// 向错误日志列表中追加一条错误记录。
        /// </summary>
        /// <param name="message">错误描述信息（建议包含行号）</param>
        public void AddError(string message)
        {
            ErrorLogs.Add(message);
        }
    }
}