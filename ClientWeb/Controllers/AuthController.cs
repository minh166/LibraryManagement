using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using ClientWeb.Models;
public class AuthController : Controller
{
    private readonly HttpClient _http;

    public AuthController(IHttpClientFactory factory)
    {
        _http = factory.CreateClient();
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

        var response = await _http.PostAsync("http://localhost:5100/api/user/login", content);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Invalid username or password";
            return View();
        }

        var result = await response.Content.ReadAsStringAsync();

        var user = JsonSerializer.Deserialize<JsonElement>(result);

        int userId = user.GetProperty("id").GetInt32();
        int role = user.GetProperty("role").GetInt32();

        // 🔥 LƯU SESSION
        HttpContext.Session.SetInt32("UserId", userId);
        HttpContext.Session.SetInt32("Role", role);

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}