namespace GolestanSystem.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Title { get; set; }
        public int Credits { get; set; }
        public string? Description { get; set; }
        public DateTime ExamTime { get; set; }
        public int? FacultyId { get; set; }
        public Faculty? Faculty { get; set; }
        public ICollection<CourseClass>? Classes { get; set; }
        public ICollection<Prerequisite>? Prerequisites { get; set; }
    }
}