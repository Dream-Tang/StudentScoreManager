
namespace StudentScoreManager.Models
{
    public class StudentScoreRow
    {
        public int StudentNo { get; set; } // 序号 (只读显示)
        public string StudentID { get; set; } // 学号 (只读显示)
        public string StudentName { get; set; } // 姓名 (只读显示)
        public string ClassName { get; set; } // 班级 (备用，后台查询可能需要)

        // 评分维度
        public string Rule1 { get; set; } // 考勤：存储 "到", "缺", "迟到", "早退"
        public int? Rule2 { get; set; }   // 纪律：0-100，可空
        public int? Rule3 { get; set; }      // 质量：0-100，可空
    }
}
