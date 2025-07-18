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

    public class StudentInfoViewModel
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string EntryDate { get; set; }
        public string FacultyName { get; set; }
        public string RegisterDate { get; set; }
    }
}