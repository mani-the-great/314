using System.ComponentModel.DataAnnotations;

namespace GolestanSystem.Models
{
    public class Student : User
    {
        [Required(ErrorMessage = "شماره دانشجویی الزامی است")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "شماره دانشجویی باید 9 رقمی باشد")]
        [RegularExpression(@"^(40[0-9]|4[1-9][0-9])\d{6}$",
        ErrorMessage = "شماره دانشجویی باید با 400 تا 499 شروع شود")]
        public string? StudentId { get; set; }
        public DateTime EntryDate { get; set; }
        public int? FacultyId { get; set; }
        public Faculty? Faculty { get; set; }
        public ICollection<CourseStudent>? CourseStudents { get; set; }
    }
}
