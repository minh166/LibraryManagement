namespace WebAPI.DTOs
{
    public class CreateUserDTO
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int Role { get; set; } // 1 Admin, 2 Librarian, 3 Borrower
    }
}
