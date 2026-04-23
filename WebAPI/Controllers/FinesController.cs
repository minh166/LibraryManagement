using WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static WebAPI.DTOs.FineDTO;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinesController : ControllerBase
    {
        private readonly LibrarydbContext _context;

        public FinesController(LibrarydbContext context)
        {
            _context = context;
        }

        // GET: api/fines
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var fines = await _context.Fines
                .Include(f => f.BorrowRecord)
                    .ThenInclude(b => b.Book)
                .Select(f => new FineResponseDTO
                {
                    Id = f.Id,
                    BorrowRecordId = f.BorrowRecordId,
                    BookTitle = f.BorrowRecord.Book.Title,
                    Amount = f.Amount,
                    IsPaid = f.IsPaid,
                    PaidDate = f.PaidDate
                }).ToListAsync();

            return Ok(fines);
        }

        // GET: api/fines/unpaid
        [HttpGet("unpaid")]
        public async Task<IActionResult> GetUnpaid()
        {
            var fines = await _context.Fines
                .Include(f => f.BorrowRecord)
                    .ThenInclude(b => b.Book)
                .Where(f => !f.IsPaid)
                .Select(f => new FineResponseDTO
                {
                    Id = f.Id,
                    BorrowRecordId = f.BorrowRecordId,
                    BookTitle = f.BorrowRecord.Book.Title,
                    Amount = f.Amount,
                    IsPaid = f.IsPaid,
                    PaidDate = f.PaidDate
                }).ToListAsync();

            return Ok(fines);
        }

        // PUT: api/fines/5/pay
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> Pay(int id)
        {
            var fine = await _context.Fines.FindAsync(id);
            if (fine == null) return NotFound("Không tìm thấy khoản phạt");
            if (fine.IsPaid) return BadRequest("Khoản phạt đã được thanh toán");

            fine.IsPaid = true;
            fine.PaidDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok("Thanh toán phạt thành công");
        }
        // GET: api/fines/user/5
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var fines = await _context.Fines
                .Include(f => f.BorrowRecord)
                    .ThenInclude(b => b.Book)
                .Where(f => f.BorrowRecord.UserId == userId)
                .Select(f => new FineResponseDTO
                {
                    Id = f.Id,
                    BorrowRecordId = f.BorrowRecordId,
                    BookTitle = f.BorrowRecord.Book.Title,
                    Amount = f.Amount,
                    IsPaid = f.IsPaid,
                    PaidDate = f.PaidDate
                })
                .ToListAsync();

            return Ok(fines);
        }
        // GET: api/fines/user/5/unpaid
        [HttpGet("user/{userId}/unpaid")]
        public async Task<IActionResult> GetUnpaidByUser(int userId)
        {
            var fines = await _context.Fines
                .Include(f => f.BorrowRecord)
                    .ThenInclude(b => b.Book)
                .Where(f => f.BorrowRecord.UserId == userId && !f.IsPaid)
                .Select(f => new FineResponseDTO
                {
                    Id = f.Id,
                    BorrowRecordId = f.BorrowRecordId,
                    BookTitle = f.BorrowRecord.Book.Title,
                    Amount = f.Amount,
                    IsPaid = f.IsPaid,
                    PaidDate = f.PaidDate
                })
                .ToListAsync();

            return Ok(fines);
        }
    }
}
