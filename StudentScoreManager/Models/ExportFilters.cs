using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentScoreManager.Models
{
    // 通用筛选基类（可选）
    public class BaseFilter
    {
        public string? Keyword { get; set; } // 通用关键词搜索
    }

    // 学生导出筛选
    public class StudentExportFilter : BaseFilter
    {
        public string? ClassName { get; set; } // 指定班级
        public string? Grade { get; set; }     // 指定年级（如果表里有）
    }

    // 教学日志导出筛选
    public class TeachingLogExportFilter : BaseFilter
    {
        public string? ClassName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CourseName { get; set; }
    }

    // 班级导出筛选
    public class ClassExportFilter : BaseFilter
    {
        // 目前班级可能只需要关键词搜索
    }
}
