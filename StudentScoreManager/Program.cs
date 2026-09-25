// 文件路径: Program.cs

namespace StudentScoreManager
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 开启系统 DPI 感知（推荐）
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            // 启用应用程序的视觉样式（WinForms 标准配置）
            Application.EnableVisualStyles();

            // 设置文本默认呈现方式
            Application.SetCompatibleTextRenderingDefault(false);

            // 启动主窗体。
            // 所有的数据库初始化、依赖注入逻辑都已在 MainForm 中处理。
            Application.Run(new MainForm());
        }
    }
}