using ClientWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace ClientWeb.Controllers
{
    public class UsersController : Controller
    {
        private readonly IHttpClientFactory _http;

        public UsersController(IHttpClientFactory http)
        {
            _http = http;
        }

        private HttpClient Client() => _http.CreateClient("LibraryAPI");

        private IActionResult? CheckAdmin()
        {
            var role = HttpContext.Session.GetInt32("Role");
            if (role != 1)
                return RedirectToAction("Login", "Auth");

            return null;
        }

        // LIST
        public async Task<IActionResult> Index(int page = 1)
        {
            var res = await Client().GetAsync($"api/user?page={page}&pageSize=5");
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                return Content("API ERROR: " + json);

            var root = JsonSerializer.Deserialize<JsonElement>(json);

            // Lấy paging info
            ViewBag.Total = root.GetProperty("total").GetInt32();
            ViewBag.Page = root.GetProperty("page").GetInt32();
            ViewBag.PageSize = root.GetProperty("pageSize").GetInt32();

            // 🔥 LẤY ĐÚNG data[]
            var users = JsonSerializer.Deserialize<List<UserViewModel>>(
                root.GetProperty("data").ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(users);
        }
        //CREATE
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreteUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = Client();

            var body = new
            {
                username = model.Username,
                password = model.Password,
                fullName = model.FullName,
                email = model.Email,
                phone = model.Phone,
                role = model.Role
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            // nếu API cần userId (admin)
            var userId = HttpContext.Session.GetInt32("UserId");

            var response = await client.PostAsync($"api/user?userId={userId}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Tạo user thành công!";
                return RedirectToAction(nameof(Index));
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", error);

            return View(model);
        }
        // DETAIL
        public async Task<IActionResult> Detail(int id)
        {
            var res = await Client().GetAsync($"api/user/{id}");
            var json = await res.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<UserViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(user);
        }

        // DISABLE
        [HttpPost]
        public async Task<IActionResult> Disable(int id)
        {
            await Client().PutAsync($"api/user/{id}/disable", null);
            return RedirectToAction("Index");
        }

        // ENABLE
        [HttpPost]
        public async Task<IActionResult> Enable(int id)
        {
            await Client().PutAsync($"api/user/{id}/enable", null);
            return RedirectToAction("Index");
        }
    }
}
