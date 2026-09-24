using StudentScoreManager.Services;

namespace StudentScoreManager
{
    public partial class MainForm : Form
    {
        // 声明全局服务实例
        // 使用 null! 告诉编译器：该字段将在后续（如 Load 事件中）被安全地初始化，消除警告
        private DbHelper _dbHelper = null!;
        private ImportService _importService = null!;

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 主窗体加载事件：初始化数据库和服务依赖
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // 1. 全局唯一实例化 DbHelper（请确保路径正确）
                _dbHelper = new DbHelper("scores.db");

                // 2. 初始化数据库表结构
                var dbInitializer = new DatabaseInitializer(_dbHelper);
                dbInitializer.InitializeTables();

                // 3. 初始化导入服务（将 DbHelper 注入）
                _importService = new ImportService(_dbHelper);

                MessageBox.Show("系统初始化完成，数据库已就绪。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"系统启动失败：{ex.Message}", "致命错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }


        /// <summary>
        /// 下载模板按钮点击事件
        /// </summary>
        private void btnDownloadTemplate_Click(object sender, EventArgs e)
        {
            // 实现下载模板的逻辑
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "保存模板";
                sfd.Filter = "Excel 文件 (*.xlsx)|*.xlsx";
                sfd.FileName = "学生信息导入模板.xlsx";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 调用服务生成模板并保存
                        var templateService = new TemplateService();
                        templateService.GenerateTemplate(sfd.FileName);
                        //templateService.SaveAs(sfd.FileName);
                        //templateService.Dispose();
                        MessageBox.Show("模板已成功保存，请打开填写后上传。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"下载模板失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        /// <summary>
        /// 导入按钮点击事件
        /// </summary>
        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Excel文件|*.xlsx";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 直接调用已注入好依赖的导入服务
                    var result = _importService.ImportAll(openFileDialog.FileName, overwriteExisting:chkOverwrite.Checked);

                    if (result.IsSuccess)
                    {
                        MessageBox.Show($"导入成功！共处理 {result.TotalSuccessCount} 条记录。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string errorMsg = "导入结束，存在以下错误：\n\n" + string.Join("\n", result.ErrorLogs);
                        MessageBox.Show(errorMsg, "导入结束（含错误）", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }
    }

}