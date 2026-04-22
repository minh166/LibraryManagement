using WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static WebAPI.DTOs.BookDTO;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LibrarydbContext _context;

        public BooksController(LibrarydbContext context)
        {
            _context = context;
        }

        // GET: api/books
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var books = await _context.Books
                .Include(b => b.Category)
                .Select(b => new BookResponseDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Description = b.Description,
                    CategoryName = b.Category.Name,
                    TotalQuantity = b.TotalQuantity,
                    AvailableQuantity = b.AvailableQuantity,
                    CreatedAt = b.CreatedAt
                }).ToListAsync();

            return Ok(books);
        }

        // GET: api/books/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound("Không tìm thấy sách");

            var dto = new BookResponseDTO
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                CategoryName = book.Category.Name,
                TotalQuantity = book.TotalQuantity,
                AvailableQuantity = book.AvailableQuantity,
                CreatedAt = book.CreatedAt
            };

            return Ok(dto);
        }

        // POST: api/books
        [HttpPost]
        public async Task<IActionResult> Create(BookRequestDTO dto)
        {
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null) return BadRequest("Thể loại không tồn tại");

            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                TotalQuantity = dto.TotalQuantity,
                AvailableQuantity = dto.TotalQuantity
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
        }

        // PUT: api/books/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BookRequestDTO dto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound("Không tìm thấy sách");

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null) return BadRequest("Thể loại không tồn tại");

            // Tính lại AvailableQuantity khi thay đổi TotalQuantity
            int borrowed = book.TotalQuantity - book.AvailableQuantity;

            book.Title = dto.Title;
            book.Author = dto.Author;
            book.Description = dto.Description;
            book.CategoryId = dto.CategoryId;
            book.TotalQuantity = dto.TotalQuantity;
            book.AvailableQuantity = dto.TotalQuantity - borrowed;

            await _context.SaveChangesAsync();
            return Ok(book);
        }

        // DELETE: api/books/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound("Không tìm thấy sách");

            // Kiểm tra còn sách đang được mượn không
            bool isBorrowed = await _context.BorrowRecords
            .AnyAsync(b => b.BookId == id &&
                     (b.Status == "Borrowing" || b.Status == "Pending"));

            if (isBorrowed) return BadRequest("Sách đang được mượn, không thể xóa");

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return Ok("Xóa sách thành công");
        }
    }
}
