// 文件: Models/TemplateModels.cs

namespace StudentScoreSystem.Models
{
    /// <summary>
    /// 班级管理表模板
    /// </summary>
    public class ClassTemplate
    {
        // 注意：ClassID 是自增主键，模板中不需要用户提供
        public string ClassName { get; set; } = string.Empty;
        public string Counselor { get; set; } = string.Empty;
        public int StudentCount { get; set; } = 0;
    }

    /// <summary>
    /// 学生信息表模板
    /// </summary>
    public class StudentTemplate
    {
        public string StudentID { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty; // 使用班级名称关联，比 ClassID 更友好
    }

    /// <summary>
    /// 评分规则表模板
    /// </summary>
    public class ScoringRuleTemplate
    {
        // RuleID 是自增主键，模板中不需要
        public string RuleName { get; set; } = string.Empty;
        public double MaxScore { get; set; }
        public double Weight { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// 教学日志表模板
    /// </summary>
    public class TeachingLogTemplate
    {
        // LogID 是自增主键，模板中不需要
        public string ClassName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string TeachingDate { get; set; } = string.Empty; // 使用字符串，方便用户输入如 "2026-09-10"
        public string TeachingHours { get; set; } = string.Empty;  // 授课节次
        public string TeachingContent { get; set; } = string.Empty; // 课程内容（可空）
        public string Classroom { get; set; } = string.Empty;       // 教室
    }
}