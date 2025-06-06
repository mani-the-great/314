namespace GolestanSystem.Models
{
    public class Professor : User
    {
        public string? ProfessorId { get; set; }
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }
        public int? FacultyId { get; set; }
        public Faculty? Faculty { get; set; }
        public string? Specialization { get; set; }
        public ICollection<CourseProfessor>? CourseProfessors { get; set; }
    }
}
