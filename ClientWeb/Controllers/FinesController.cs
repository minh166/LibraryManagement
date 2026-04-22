using ClientWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ClientWeb.Controllers
{
    public class FinesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public FinesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetClient() => _httpClientFactory.CreateClient("LibraryAPI");

        // GET: /Fines
        public async Task<IActionResult> Index()
        {
            var client = GetClient();
            var response = await client.GetAsync("api/fines");
            var json = await response.Content.ReadAsStringAsync();
            var fines = JsonSerializer.Deserialize<List<FineViewModel>>(json, _jsonOptions) ?? new();
            return View(fines);
        }

        // GET: /Fines/Unpaid
        public async Task<IActionResult> Unpaid()
        {
            var client = GetClient();
            var response = await client.GetAsync("api/fines/unpaid");
            var json = await response.Content.ReadAsStringAsync();
            var fines = JsonSerializer.Deserialize<List<FineViewModel>>(json, _jsonOptions) ?? new();
            return View(fines);
        }

        // POST: /Fines/Pay/5
        [HttpPost]
        public async Task<IActionResult> Pay(int id)
        {
            var client = GetClient();
            var response = await client.PutAsync($"api/fines/{id}/pay", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Thanh toán phạt thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
