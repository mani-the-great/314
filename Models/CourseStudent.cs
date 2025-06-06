namespace GolestanSystem.Models
{
    public class CourseStudent
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public int CourseClassId { get; set; }
        public CourseClass? CourseClass { get; set; }
        public decimal? Grade { get; set; }
    }
}
