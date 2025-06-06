namespace GolestanSystem.Models
{
    public class Faculty
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? MainBuilding { get; set; }
        public decimal Budget { get; set; }
        public ICollection<Course>? Courses { get; set; }
        public ICollection<Student>? Students { get; set; }
        public ICollection<Professor>? Professors { get; set; }
    }
}