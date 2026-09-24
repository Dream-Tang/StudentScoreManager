// 文件路径: Services/TemplateService.cs

using MiniExcelLibs;
using StudentScoreManager.Models;
using System.Collections.Generic;
using System.IO;

namespace StudentScoreManager.Services
{
    public class TemplateService
    {
        /// <summary>
        /// 生成标准导入模板 Excel 文件
        /// </summary>
        public void GenerateTemplate(string filePath)
        {
            // 准备示例数据
            var classes = new List<ClassImportModel>
            {
                new ClassImportModel { ClassName = "25电子2班", Counselor = "张婷", StudentCount = 60 },
                new ClassImportModel { ClassName = "25通信2班", Counselor = "张红", StudentCount = 58 }
            };

            var students = new List<StudentImportModel>
            {
                new StudentImportModel { StudentID = "20210001", StudentName = "张三", ClassName = "25电子2班" },
                new StudentImportModel { StudentID = "20210002", StudentName = "李四", ClassName = "25通信2班" }
            };

            var logs = new List<TeachingLogImportModel>
            {
                new TeachingLogImportModel
                {
                    ClassName = "25电子2班",
                    CourseCode = "CS101",
                    CourseName = "C#程序设计",
                    TeachingDate = "2026-09-10",
                    TeachingContent = "WinForms 基础",
                    Classroom = "A栋101",
                    TeachingHours = "一、二"
                }
            };

            var scoringRules = new List<ScoreRuleImportModel>
            {
                new ScoreRuleImportModel { RuleName = "考勤", MaxScore = 100.0, Weight = 0.3, SortOrder = 1 },
                new ScoreRuleImportModel { RuleName = "纪律", MaxScore = 100.0, Weight = 0.3, SortOrder = 2 },
                new ScoreRuleImportModel { RuleName = "质量", MaxScore = 100.0, Weight = 0.4, SortOrder = 3 }
            };

            // 组装多 Sheet 数据
            var sheets = new Dictionary<string, object>
            {
                { "班级管理", classes },
                { "学生信息", students },
                { "评分规则", scoringRules },
                { "教学日志", logs }
            };

            // 确保目录存在
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 生成 Excel 文件
            MiniExcel.SaveAs(filePath, sheets, overwriteFile: true);
        }
    }
}