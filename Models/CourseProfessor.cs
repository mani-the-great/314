namespace GolestanSystem.Models
{
    public class CourseProfessor
    {
        public int Id { get; set; }
        public int ProfessorId { get; set; }
        public Professor? Professor { get; set; }
        public int CourseClassId { get; set; }
        public CourseClass? CourseClass { get; set; }
    }
}
