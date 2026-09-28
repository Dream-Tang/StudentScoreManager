
using StudentScoreManager.Models;
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
                    var result = _importService.ImportAll(openFileDialog.FileName, overwriteExisting: chkOverwrite.Checked);

                    if (result.IsSuccess)
                    {
                        MessageBox.Show($"导入完成！新增 {result.Inserted} 条，更新 {result.Updated} 条，跳过 {result.Skipped} 条。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // 失败提示分区：先给汇总，再逐条列出“失败明细（具体到行、含业务主键）”，
                        // 最后附“说明”（如某表已整体回滚），避免把回滚提示当成一条失败记录、也便于定位“是谁失败”。
                        string errorMsg = $"导入结束：新增 {result.Inserted} 条，更新 {result.Updated} 条，跳过 {result.Skipped} 条，失败 {result.ErrorLogs.Count} 条。";

                        if (result.ErrorLogs.Count > 0)
                        {
                            errorMsg += $"\n\n【失败明细】（共 {result.ErrorLogs.Count} 条，已定位到具体行）\n" + string.Join("\n", result.ErrorLogs);
                        }

                        if (result.Notices.Count > 0)
                        {
                            errorMsg += "\n\n【说明】\n" + string.Join("\n", result.Notices);
                        }

                        MessageBox.Show(errorMsg, "导入结束（含错误）", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void BtnConfirmExport_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"导出报表_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 1. 从 UI 控件收集筛选条件
                    var studentFilter = new StudentExportFilter
                    {
                        ClassName = cmbClass.SelectedItem?.ToString(), // 如果选的是"全部"，这里可能是 null 或 empty
                        Keyword = txtStudentKeyword.Text
                    };

                    // 处理"全部"的情况
                    if (studentFilter.ClassName == "全部") studentFilter.ClassName = null;

                    var logFilter = new TeachingLogExportFilter
                    {
                        ClassName = cmbLogClass.SelectedItem?.ToString(),
                        CourseName = txtCourseKeyword.Text,
                        StartDate = dpStartDate.Value,
                        EndDate = dpEndDate.Value
                    };

                    if (logFilter.ClassName == "全部") logFilter.ClassName = null;

                    // 2. 调用服务
                    var exportService = new ExportService(_dbHelper);

                    // 显示加载状态
                    //this.IsEnabled = false;

                    bool success = exportService.ExportAllToExcel(
                        saveFileDialog.FileName,
                        classFilter: null, // 班级暂时不筛选
                        studentFilter: studentFilter,
                        logFilter: logFilter
                    );

                    if (success)
                    {
                        MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    //this.IsEnabled = true;
                }
            }
        }

        private void btnStartUse_Click(object sender, EventArgs e)
        {
            Form f1 = new Forms.Form1();
            f1.Show();
        }

        /// <summary>
        /// 清理重复教学日志按钮点击事件。
        /// 一次性清理历史遗留的重复教学日志（去重键：班级+课程名+授课日期+节次），
        /// 每个业务键只保留最早入库的一条，其余删除。
        /// </summary>
        private void btnCleanupDuplicates_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "将按【班级 + 课程名 + 授课日期 + 节次】清理重复的教学日志，每个组合只保留最早的一条，其余删除。\n该操作会直接修改数据库且不可撤销，是否继续？",
                "清理重复教学日志", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                int deleted = _importService.CleanupDuplicateTeachingLogs();
                if (deleted > 0)
                    MessageBox.Show($"清理完成，共删除 {deleted} 条重复教学日志。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("未发现重复的教学日志，无需清理。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清理失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}