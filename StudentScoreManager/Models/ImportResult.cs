
// 文件路径: Models/ImportResult.cs
using System.Collections.Generic;

namespace StudentScoreManager.Models
{
    /// <summary>
    /// 数据导入操作的结果封装类。
    /// 采用三态模型：新增(Inserted) / 跳过(Skipped) / 失败(Errors)，
    /// 使"命中已有数据→跳过"不再被误判为"错误"，从而不否决事务提交。
    /// </summary>
    public class ImportResult
    {
        /// <summary>
        /// 获取或设置导入操作是否成功。
        /// 判定规则（P0-4）：只要没有"真正的异常"即为成功——跳过不算失败。
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 新增成功写入数据库的记录数（INSERT 或覆盖模式下的 UPDATE）。
        /// </summary>
        public int Inserted { get; set; }

        /// <summary>
        /// 因记录已存在而被跳过的记录数（不覆盖模式）。
        /// </summary>
        public int Skipped { get; set; }

        /// <summary>
        /// 覆盖导入模式下，因记录已存在而执行更新(UPDATE)的记录数。
        /// </summary>
        public int Updated { get; set; }

        /// <summary>
        /// 兼容旧字段：等于新增成功数，供尚未改造的调用方使用。
        /// </summary>
        public int TotalSuccessCount { get; set; }

        /// <summary>
        /// 真正的异常/失败日志列表（不含"跳过"）。每条通常包含行号和具体原因。
        /// </summary>
        public List<string> ErrorLogs { get; set; } = new List<string>();

        /// <summary>
        /// 跳过明细列表（命中已有数据、按规则跳过的记录），仅用于展示，不影响成功判定。
        /// </summary>
        public List<string> SkippedLogs { get; set; } = new List<string>();

        /// <summary>
        /// 向错误日志列表中追加一条"真异常"记录，并计数。
        /// </summary>
        /// <param name="message">错误描述信息（建议包含行号）</param>
        public void AddError(string message)
        {
            ErrorLogs.Add(message);
        }

        /// <summary>
        /// 记录一条"跳过"（命中已有数据），与失败区分，不否决提交。
        /// </summary>
        /// <param name="message">跳过描述信息（建议包含行号）</param>
        public void AddSkipped(string message)
        {
            Skipped++;
            SkippedLogs.Add(message);
        }

        /// <summary>
        /// 说明性信息列表（如“某表因错误已整体回滚”、“未读取到有效数据”等）。
        /// 这类信息用于解释结果，但不作为单条记录计入“失败条数”，避免回滚提示与失败明细重复计数。
        /// </summary>
        public List<string> Notices { get; set; } = new List<string>();

        /// <summary>
        /// 是否发生过“整表回滚”。一旦某表被回滚，即便没有逐行失败明细，也应视为整体失败。
        /// </summary>
        public bool HasRolledBack { get; set; }

        /// <summary>
        /// 追加一条说明性信息（不计入失败条数，仅作展示与解释）。
        /// </summary>
        public void AddNotice(string message)
        {
            Notices.Add(message);
        }
    }
}