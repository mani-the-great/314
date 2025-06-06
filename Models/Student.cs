namespace GolestanSystem.Models
{
    public class Student : User
    {
        public string? StudentId { get; set; }
        public DateTime EntryDate { get; set; }
        public int? FacultyId { get; set; }
        public Faculty? Faculty { get; set; }
        public ICollection<CourseStudent>? CourseStudents { get; set; }
    }
}
