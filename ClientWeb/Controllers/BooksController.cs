using ClientWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Text.Json;

namespace ClientWeb.Controllers
{
    public class BooksController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public BooksController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetClient() => _httpClientFactory.CreateClient("LibraryAPI");

        // GET: /Books
        public async Task<IActionResult> Index(string? search)
        {
            var client = GetClient();
            var response = await client.GetAsync("api/books");
            var json = await response.Content.ReadAsStringAsync();
            var books = JsonSerializer.Deserialize<List<BookViewModel>>(json, _jsonOptions) ?? new();

            // Tìm kiếm phía client
            if (!string.IsNullOrEmpty(search))
            {
                books = books.Where(b =>
                    b.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    b.Author.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            ViewBag.Search = search;
            return View(books);
        }

        // GET: /Books/Create
        public async Task<IActionResult> Create()
        {
            await LoadCategories();
            return View();
        }

        // POST: /Books/Create
        [HttpPost]
        public async Task<IActionResult> Create(BookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            var client = GetClient();
            var body = new
            {
                title = model.Title,
                author = model.Author,
                description = model.Description,
                categoryId = model.CategoryId,
                totalQuantity = model.TotalQuantity
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("api/books", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Thêm sách thất bại");
            await LoadCategories();
            return View(model);
        }

        // GET: /Books/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = GetClient();
            var response = await client.GetAsync($"api/books/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var book = JsonSerializer.Deserialize<BookViewModel>(json, _jsonOptions);

            await LoadCategories();
            return View(book);
        }

        // POST: /Books/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, BookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            var client = GetClient();
            var body = new
            {
                title = model.Title,
                author = model.Author,
                description = model.Description,
                categoryId = model.CategoryId,
                totalQuantity = model.TotalQuantity
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync($"api/books/{id}", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Cập nhật sách thất bại");
            await LoadCategories();
            return View(model);
        }

        // POST: /Books/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"api/books/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = error;
            }

            return RedirectToAction(nameof(Index));
        }

        // Load danh sách thể loại vào ViewBag
        private async Task LoadCategories()
        {
            var client = GetClient();
            var response = await client.GetAsync("api/categories");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(json, _jsonOptions) ?? new();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
            }
        }
    }
}
