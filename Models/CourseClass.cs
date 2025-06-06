namespace GolestanSystem.Models
{
    public class CourseClass
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public string? Building { get; set; }
        public string? RoomNumber { get; set; }
        public int Capacity { get; set; }
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public ICollection<CourseStudent>? Students { get; set; }
        public ICollection<CourseProfessor>? Professors { get; set; }
    }
}