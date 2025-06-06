namespace GolestanSystem.Models
{
    public abstract class User
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public DateTime RegisterDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
    }
}