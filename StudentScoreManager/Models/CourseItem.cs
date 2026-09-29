
// Models/CourseItem.cs
namespace StudentScoreManager.Models
{
    // 承载一条 TeachingLog，同时用于下拉框显示 + 课程信息确认表
    public class CourseItem
    {
        public int? LogID { get; set; }     // 有课时从库取，无课时为null
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? TeachingHours { get; set; }
        public string? ClassName { get; set; }
        public string? Classroom { get; set; }
        public string? Content { get; set; }
        // 备注：仅用于 Form1 左侧课程卡片展示；若 TeachingLogs 无该列则读为空串
        public string? Remark { get; set; } = "";
    }
}