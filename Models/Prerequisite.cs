namespace GolestanSystem.Models
{
    public class Prerequisite
    {
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public int PrerequisiteCourseId { get; set; }
        public Course? PrerequisiteCourse { get; set; }
    }
}