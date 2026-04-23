using ClientWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Text.Json;

namespace ClientWeb.Controllers
{
    public class BorrowsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public BorrowsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetClient() => _httpClientFactory.CreateClient("LibraryAPI");

        // GET: /Borrows
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var client = GetClient();
            var response = await client.GetAsync($"api/borrows?userId={userId}");
            var json = await response.Content.ReadAsStringAsync();
            var borrows = JsonSerializer.Deserialize<List<BorrowViewModel>>(json, _jsonOptions) ?? new();
            return View(borrows);
        }

        // GET: /Borrows/Overdue
        public async Task<IActionResult> Overdue()
        {
            var client = GetClient();
            var response = await client.GetAsync("api/borrows/overdue");
            var json = await response.Content.ReadAsStringAsync();
            var borrows = JsonSerializer.Deserialize<List<BorrowViewModel>>(json, _jsonOptions) ?? new();
            return View(borrows);
        }

        // GET: /Borrows/Create
        public async Task<IActionResult> Create()
        {
            await LoadBooks();
            await LoadUsers();
            return View();
        }

        // POST: /Borrows/Create
        [HttpPost]
        public async Task<IActionResult> Create(BorrowViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadBooks();
                await LoadUsers();
                return View(model);
            }

            var client = GetClient();
            var body = new
            {
                userId = model.UserId,
                bookId = model.BookId,
                dueDate = model.DueDate
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("api/borrows", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", error);
            await LoadBooks();
            await LoadUsers();
            return View(model);
        }

        // POST: /Borrows/Confirm/5
        [HttpPost]
        public async Task<IActionResult> Confirm(int id)
        {
            var client = GetClient();
            var response = await client.PutAsync($"api/borrows/{id}/confirm", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Xác nhận mượn sách thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Borrows/Return/5
        [HttpPost]
        public async Task<IActionResult> Return(int id)
        {
            var client = GetClient();
            var response = await client.PutAsync($"api/borrows/{id}/return", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Trả sách thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Borrows/Extend/5
        public async Task<IActionResult> Extend(int id)
        {
            var client = GetClient();
            var response = await client.GetAsync("api/borrows");
            var json = await response.Content.ReadAsStringAsync();
            var borrows = JsonSerializer.Deserialize<List<BorrowViewModel>>(json, _jsonOptions) ?? new();
            var borrow = borrows.FirstOrDefault(b => b.Id == id);
            if (borrow == null) return NotFound();
            return View(borrow);
        }

        // POST: /Borrows/Extend/5
        [HttpPost]
        public async Task<IActionResult> Extend(int id, DateTime newDueDate)
        {
            var client = GetClient();
            var body = new { newDueDate };
            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync($"api/borrows/{id}/extend", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = error;
                return RedirectToAction(nameof(Extend), new { id });
            }

            TempData["Success"] = "Gia hạn thành công!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadBooks()
        {
            var client = GetClient();
            var response = await client.GetAsync("api/books");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var books = JsonSerializer.Deserialize<List<BookViewModel>>(json, _jsonOptions) ?? new();
                ViewBag.Books = new SelectList(books.Where(b => b.AvailableQuantity > 0), "Id", "Title");
            }
        }

        private async Task LoadUsers()
        {
            var client = GetClient();
            var response = await client.GetAsync("api/users");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserViewModel>>(json, _jsonOptions) ?? new();
                ViewBag.Users = new SelectList(users, "Id", "FullName");
            }
        }
        private IActionResult? CheckLogin()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Auth");

            return null;
        }
        [HttpPost]
        public async Task<IActionResult> Borrow(BorrowViewModel model)
        {
            var check = CheckLogin();
            if (check != null) return check;

            if (!ModelState.IsValid)
            {
                await LoadBooks();
                return View(model);
            }

            var userId = HttpContext.Session.GetInt32("UserId");

            var client = GetClient();

            var body = new
            {
                userId = userId,
                bookId = model.BookId,
                dueDate = model.DueDate
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync("api/borrows", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Mượn sách thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = await response.Content.ReadAsStringAsync();

            await LoadBooks();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> BorrowBook(int bookId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var client = GetClient();

            var response = await client.PostAsync(
                $"api/borrows/borrow?userId={userId}&bookId={bookId}",
                null
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Đã gửi yêu cầu mượn sách!";
            }

            return RedirectToAction("Index", "Books");
        }
        // GET: /Borrows/MyBorrows
        public async Task<IActionResult> MyBorrows()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var client = GetClient();

            var response = await client.GetAsync($"api/borrows/user/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không load được lịch sử mượn";
                return View(new List<BorrowViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var borrows = JsonSerializer.Deserialize<List<BorrowViewModel>>(json, _jsonOptions)
                          ?? new List<BorrowViewModel>();

            return View(borrows);
        }
    }
}
