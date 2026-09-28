
// 文件路径: Services/AppConfig.cs
using System;
using System.IO;

namespace StudentScoreManager.Services
{
    /// <summary>
    /// 全局应用配置：集中管理程序内所有外部依赖路径（当前为 SQLite 数据库文件路径）。
    /// 目的：全程序只认这一处数据库路径，避免各处硬编码 "scores.db" 因当前工作目录不确定
    ///       而连到 bin\Debug 下的空库。所有需要 new DbHelper(...) 的地方都应引用 AppConfig.DbPath。
    /// </summary>
    public static class AppConfig
    {
        /// <summary>
        /// SQLite 数据库文件的绝对路径。
        /// 基于 AppContext.BaseDirectory（= 可执行文件所在目录）拼接，
        /// 不受运行时"当前工作目录"影响，稳定指向随程序一起的那份库文件。
        /// 如需改库文件名/位置，只改这一处即可，全程序生效。
        /// </summary>
        public static readonly string DbPath =
            Path.Combine(AppContext.BaseDirectory, "scores.db");
    }
}