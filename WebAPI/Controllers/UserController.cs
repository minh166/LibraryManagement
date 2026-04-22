using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController: ControllerBase
    {
        private readonly LibrarydbContext _context;

        public UserController(LibrarydbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll(int userId)
        {
            if (!IsAdmin(userId))
                return Unauthorized("Only admin can access");

            var users = _context.Users
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToList();

            return Ok(users);
        }

        // =========================
        // 2. USER DETAIL
        // =========================
        [HttpGet("{id}")]
        public IActionResult GetById(int id, int userId)
        {
            if (!IsAdmin(userId))
                return Unauthorized("Only admin can access");

            var user = _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email=u.Email,
                    Phone=u.Phone,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefault();

            if (user == null) return NotFound();

            return Ok(user);
        }

        // =========================
        // 3. CREATE USER
        // =========================
        [HttpPost]
        public IActionResult Create(int userId, CreateUserDTO dto)
        {
            if (!IsAdmin(userId))
                return Unauthorized("Only admin can access");

            if (_context.Users.Any(u => u.Username == dto.Username))
                return BadRequest("Username already exists");

            var user = new User
            {
                Username = dto.Username,
                Password = dto.Password,
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "User created" });
        }

        // =========================
        // 4. SEARCH USER
        // =========================
        [HttpGet("search")]
        public IActionResult Search(string keyword, int userId)
        {
            if (!IsAdmin(userId))
                return Unauthorized("Only admin can access");

            var users = _context.Users
                .Where(u => u.Username.Contains(keyword) || u.FullName.Contains(keyword))
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Role = u.Role,
                    Email = u.Email,
                    Phone = u.Phone,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToList();

            return Ok(users);
        }

        // =========================
        // 5. DISABLE USER
        // =========================
        [HttpPut("{id}/disable")]
        public IActionResult Disable(int id, int userId)
        {
            if (!IsAdmin(userId))
                return Unauthorized("Only admin can access");

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            user.IsActive = false;

            _context.SaveChanges();

            return Ok(new { message = "User disabled" });
        }

        // =========================
        // 6. ENABLE USER
        // =========================
        [HttpPut("{id}/enable")]
        public IActionResult Enable(int id, int userId)
        {
            if (!IsAdmin(userId))
                return Unauthorized("Only admin can access");

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            user.IsActive = true;

            _context.SaveChanges();

            return Ok(new { message = "User enabled" });
        }
        private bool IsAdmin(int userId)
        {
            var user = _context.Users.Find(userId);
            return user != null && user.Role == 1;
        }
        //7: LOGIN
        [HttpPost("login")]
        public IActionResult Login(LoginDTO dto)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == dto.Username && u.Password == dto.Password);

            if (user == null)
                return Unauthorized("Invalid username or password");

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Role, 
                user.IsActive
            });
        }
        //8: REGISTER
        [HttpPost("register")]
        public IActionResult Register(RegisterDTO dto)
        {
            // 1. Validate cơ bản
            if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
                return BadRequest("Username and Password are required");

            // 2. Check username tồn tại
            if (_context.Users.Any(u => u.Username == dto.Username))
                return BadRequest("Username already exists");

            // (Optional) Check email trùng
            if (!string.IsNullOrEmpty(dto.Email) && _context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already exists");

            // 3. Tạo user
            var user = new User
            {
                Username = dto.Username,
                Password = dto.Password,
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = 3, 
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Register success",
                user.Id,
                user.Username,
                user.Email,
                user.Phone,
                user.Role
            });
        }
    }
}
