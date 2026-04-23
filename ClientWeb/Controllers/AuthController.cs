using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using ClientWeb.Models;
public class AuthController : Controller
{
    private readonly HttpClient _http;

    public AuthController(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("LibraryAPI");
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("api/user/login", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            ViewBag.Error = error; // hiển thị đúng message từ API
            return View();
        }

        var result = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<JsonElement>(result);

        int userId = user.GetProperty("id").GetInt32();
        int role = user.GetProperty("role").GetInt32();
        string username = user.GetProperty("username").GetString() ?? "";

        // 🔥 QUAN TRỌNG: lấy IsActive
        bool isActive = user.GetProperty("isActive").GetBoolean();

        // 🔥 CHẶN NGAY TẠI ĐÂY
        if (!isActive)
        {
            ViewBag.Error = "Tài khoản đã bị khóa";
            return View();
        }

        // lưu session
        HttpContext.Session.SetInt32("UserId", userId);
        HttpContext.Session.SetInt32("Role", role);
        HttpContext.Session.SetString("Username", username);
        HttpContext.Session.SetInt32("IsActive", isActive ? 1 : 0);

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}