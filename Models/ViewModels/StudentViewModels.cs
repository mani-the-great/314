namespace GolestanSystem.Models.ViewModels
{
    public class StudentCourseViewModel
    {
        public string CourseTitle { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string ClassTime { get; set; } = string.Empty;
        public DateTime ExamTime { get; set; }
        public string Professors { get; set; } = string.Empty;
        public int CourseClassId { get; set; }
    }

    public class StudentGradeViewModel
    {
        public string CourseTitle { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public int Credits { get; set; }
        public decimal Grade { get; set; }
        public bool IsPassed { get; set; }
    }
}