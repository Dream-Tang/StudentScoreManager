// 文件路径: Models/ImportModels.cs

using MiniExcelLibs.Attributes;

namespace StudentScoreManager.Models
{
    /// <summary>
    /// 映射 Excel "班级管理" 工作表的数据模型
    /// </summary>
    public class ClassImportModel
    {
        [ExcelColumnName("班级名称")]
        public string? ClassName { get; set; }

        [ExcelColumnName("辅导员")]
        public string? Counselor { get; set; }

        [ExcelColumnName("班级人数")]
        public int? StudentCount { get; set; }
    }

    /// <summary>
    /// 映射 Excel "学生信息" 工作表的数据模型
    /// </summary>
    public class StudentImportModel
    {
        [ExcelColumnName("学号")]
        public string? StudentID { get; set; }

        [ExcelColumnName("姓名")]
        public string? StudentName { get; set; }

        [ExcelColumnName("所属班级")]
        public string? ClassName { get; set; }
    }

    /// <summary>
    /// 映射 Excel "评分规则" 工作表的数据模型
    /// </summary>
    public class ScoreRuleImportModel
    {
        [ExcelColumnName("评分维度")]
        public string? RuleName { get; set; }

        [ExcelColumnName("最高评分")]
        public double? MaxScore { get; set; }

        [ExcelColumnName("权重")]
        public double? Weight { get; set; }

        [ExcelColumnName("排序")]
        public int? SortOrder { get; set; }
    }

    /// <summary>
    /// 映射 Excel "教学日志" 工作表的数据模型
    /// </summary>
    public class TeachingLogImportModel
    {
        [ExcelColumnName("班级名称")]
        public string? ClassName { get; set; }

        [ExcelColumnName("课程代码")]
        public string? CourseCode { get; set; }

        [ExcelColumnName("课程名称")]
        public string? CourseName { get; set; }

        [ExcelColumnName("授课日期")]
        public DateTime? TeachingDate { get; set; }

        [ExcelColumnName("节次")]
        public string? TeachingHours { get; set; }

        [ExcelColumnName("教学内容")]
        public string? TeachingContent { get; set; }

        [ExcelColumnName("教室")]
        public string? Classroom { get; set; }
    }


}