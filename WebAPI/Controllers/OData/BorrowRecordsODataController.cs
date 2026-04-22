using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Controllers.OData
{
    public class BorrowRecordsODataController : ODataController
    {
        private readonly LibrarydbContext _context;

        public BorrowRecordsODataController(LibrarydbContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.BorrowRecords
                .Include(b => b.Book)
                .Include(b => b.User)
                .Include(b => b.Fine));
        }
    }
}
