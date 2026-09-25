// Models/CourseItem.cs
namespace StudentScoreManager.Models
{
    // 承载一条 TeachingLog，同时用于下拉框显示 + 课程信息确认表
    public class CourseItem
    {
        public int LogID { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string TeachingHours { get; set; }
        public string ClassName { get; set; }
        public string Classroom { get; set; }
        public string Content { get; set; }
    }
}