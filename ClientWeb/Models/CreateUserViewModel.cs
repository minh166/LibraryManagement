namespace ClientWeb.Models
{
    public class CreteUserViewModel
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public int Role { get; set; }
    }
}
