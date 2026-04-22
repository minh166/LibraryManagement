using WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static WebAPI.DTOs.BorrowRecordDTO;
using static WebAPI.DTOs.FineDTO;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowsController : ControllerBase
    {
        private readonly LibrarydbContext _context;
        private const decimal FINE_PER_DAY = 5000;

        public BorrowsController(LibrarydbContext context)
        {
            _context = context;
        }

        // GET: api/borrows
        [HttpGet]
        public async Task<IActionResult> GetAll(int userId)
        {
            var records = await _context.BorrowRecords
        .Include(b => b.Book)
        .Include(b => b.Fine)
        .Select(b => new BorrowResponseDTO
        {
                    Id = b.Id,
                    UserId = b.UserId,
                    BookTitle = b.Book.Title,
                    Author = b.Book.Author,
                    BorrowDate = b.BorrowDate,
                    DueDate = b.DueDate,
                    ReturnDate = b.ReturnDate,
                    Status = b.Status,
                    DaysLeft = b.ReturnDate == null
                        ? (int)(b.DueDate - DateTime.Now).TotalDays
                        : 0,
                    Fine = b.Fine == null ? null : new FineResponseDTO
                    {
                        Id = b.Fine.Id,
                        BorrowRecordId = b.Fine.BorrowRecordId,
                        BookTitle = b.Book.Title,
                        Amount = b.Fine.Amount,
                        IsPaid = b.Fine.IsPaid,
                        PaidDate = b.Fine.PaidDate
                    }
                }).ToListAsync();

            return Ok(records);
        }

        // GET: api/borrows/overdue
        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdue()
        {
            var overdue = await _context.BorrowRecords
                .Include(b => b.Book)
                .Where(b => b.Status == "Borrowing" && b.DueDate < DateTime.Now)
                .Select(b => new BorrowResponseDTO
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    BookTitle = b.Book.Title,
                    Author = b.Book.Author,
                    BorrowDate = b.BorrowDate,
                    DueDate = b.DueDate,
                    Status = "Overdue",
                    DaysLeft = (int)(b.DueDate - DateTime.Now).TotalDays
                }).ToListAsync();

            return Ok(overdue);
        }

        // GET: api/borrows/user/5
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var records = await _context.BorrowRecords
                .Include(b => b.Book)
                .Include(b => b.Fine)
                .Where(b => b.UserId == userId)
                .Select(b => new BorrowResponseDTO
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    BookTitle = b.Book.Title,
                    Author = b.Book.Author,
                    BorrowDate = b.BorrowDate,
                    DueDate = b.DueDate,
                    ReturnDate = b.ReturnDate,
                    Status = b.Status,
                    DaysLeft = b.ReturnDate == null
                        ? (int)(b.DueDate - DateTime.Now).TotalDays
                        : 0,
                    Fine = b.Fine == null ? null : new FineResponseDTO
                    {
                        Id = b.Fine.Id,
                        BorrowRecordId = b.Fine.BorrowRecordId,
                        BookTitle = b.Book.Title,
                        Amount = b.Fine.Amount,
                        IsPaid = b.Fine.IsPaid,
                        PaidDate = b.Fine.PaidDate
                    }
                }).ToListAsync();

            return Ok(records);
        }

        // POST: api/borrows
        [HttpPost]
        public async Task<IActionResult> Create(BorrowRequestDTO dto)
        {
            // Kiểm tra sách tồn tại
            var book = await _context.Books.FindAsync(dto.BookId);
            if (book == null) return NotFound("Không tìm thấy sách");
            if (book.AvailableQuantity <= 0) return BadRequest("Sách đã hết, không thể mượn");

            // Kiểm tra user tồn tại
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return NotFound("Không tìm thấy người dùng");

            // Kiểm tra quá 5 cuốn chưa
            int borrowing = await _context.BorrowRecords
                .CountAsync(b => b.UserId == dto.UserId &&
                            (b.Status == "Borrowing" || b.Status == "Pending"));
            if (borrowing >= 5) return BadRequest("Đã mượn tối đa 5 cuốn sách");

            // Kiểm tra DueDate hợp lệ
            if (dto.DueDate <= DateTime.Now)
                return BadRequest("Ngày hết hạn phải sau ngày hiện tại");

            var record = new BorrowRecord
            {
                UserId = dto.UserId,
                BookId = dto.BookId,
                BorrowDate = DateTime.Now,
                DueDate = dto.DueDate,
                Status = "Pending"
            };

            _context.BorrowRecords.Add(record);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = record.Id }, record);
        }

        // PUT: api/borrows/5/confirm
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            var record = await _context.BorrowRecords
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (record == null) return NotFound("Không tìm thấy phiếu mượn");
            if (record.Status != "Pending")
                return BadRequest("Phiếu mượn không ở trạng thái chờ xác nhận");

            record.Status = "Borrowing";
            record.Book.AvailableQuantity--;

            await _context.SaveChangesAsync();
            return Ok("Xác nhận mượn sách thành công");
        }

        // PUT: api/borrows/5/return
        [HttpPut("{id}/return")]
        public async Task<IActionResult> Return(int id)
        {
            var record = await _context.BorrowRecords
                .Include(b => b.Book)
                .Include(b => b.Fine)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (record == null) return NotFound("Không tìm thấy phiếu mượn");
            if (record.Status == "Returned")
                return BadRequest("Sách đã được trả trước đó");
            if (record.Status == "Pending")
                return BadRequest("Phiếu mượn chưa được xác nhận");

            record.ReturnDate = DateTime.Now;
            record.Status = "Returned";
            record.Book.AvailableQuantity++;

            // Tính phạt nếu trả trễ
            if (record.ReturnDate > record.DueDate)
            {
                int daysLate = (int)(record.ReturnDate.Value - record.DueDate).TotalDays;
                var fine = new Fine
                {
                    BorrowRecordId = record.Id,
                    Amount = daysLate * FINE_PER_DAY,
                    IsPaid = false
                };
                _context.Fines.Add(fine);
            }

            await _context.SaveChangesAsync();
            return Ok("Trả sách thành công");
        }

        // PUT: api/borrows/5/extend
        [HttpPut("{id}/extend")]
        public async Task<IActionResult> Extend(int id, ExtendBorrowDTO dto)
        {
            var record = await _context.BorrowRecords.FindAsync(id);
            if (record == null) return NotFound("Không tìm thấy phiếu mượn");
            if (record.Status != "Borrowing")
                return BadRequest("Chỉ gia hạn được sách đang mượn");
            if (dto.NewDueDate <= record.DueDate)
                return BadRequest("Ngày gia hạn phải sau ngày hết hạn hiện tại");

            record.DueDate = dto.NewDueDate;
            await _context.SaveChangesAsync();
            return Ok("Gia hạn thành công");
        }
        private bool IsLibrarian(int userId)
        {
            var user = _context.Users.Find(userId);
            return user != null && user.Role == 2;
        }

    }
}
