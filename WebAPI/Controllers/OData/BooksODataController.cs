using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Controllers.OData
{
    public class BooksODataController : ODataController
    {
        private readonly LibrarydbContext _context;

        public BooksODataController(LibrarydbContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.Books.Include(b => b.Category));
        }

        [EnableQuery]
        public IActionResult Get(int id)
        {
            var book = _context.Books
                .Include(b => b.Category)
                .Where(b => b.Id == id);
            return Ok(book);
        }
    }
}
